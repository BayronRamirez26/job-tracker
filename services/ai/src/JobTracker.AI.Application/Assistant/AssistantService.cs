using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using JobTracker.AI.Application.Abstractions;
using JobTracker.AI.Application.Assistant.Dtos;
using JobTracker.AI.Application.Exceptions;
using JobTracker.AI.Domain.ValueObjects;

namespace JobTracker.AI.Application.Assistant;

/// <summary>
/// The application assistant use cases. Each frames a system prompt, hands the candidate profile +
/// job description to the <see cref="IAiCompletionClient"/> port, and shapes the result. Cover
/// letters and résumés come back as text; the fit assessment is strict JSON parsed here (same
/// pattern as extraction). The candidate profile is never required by the API, but every use case
/// is far more useful with one — the caller supplies the active profile.
/// </summary>
public sealed class AssistantService : IAssistantService
{
    private const string CoverLetterPrompt =
        "You write a concise, professional cover letter for a candidate applying to a specific role, " +
        "using the CANDIDATE PROFILE and JOB DESCRIPTION below. Keep it to 250–350 words in 3–4 short " +
        "paragraphs — confident but genuine — referencing concrete details from both the posting and the " +
        "candidate's real experience. Never invent facts that are not in the profile. Return ONLY the " +
        "letter body text: no preamble, no markdown, no bracketed placeholders.";

    private const string TailoredCvPrompt =
        "You produce a tailored resume for a candidate applying to a specific role. Using ONLY facts from " +
        "the CANDIDATE PROFILE, write a one-page resume in Markdown tailored to the JOB DESCRIPTION: a short " +
        "professional summary aimed at this role, a skills line that leads with the skills the posting asks " +
        "for, and experience entries whose bullet points foreground the most relevant achievements. Reorder " +
        "and rephrase for relevance, but never fabricate experience, skills, employers, or dates. Return " +
        "ONLY Markdown.";

    private const string FitPrompt =
        "You produce a fit assessment: how well a candidate matches a specific job. Return ONLY a JSON " +
        "object — no markdown fences, no commentary — with exactly these keys: \"score\" (integer 0-100, the " +
        "overall match), \"strengths\" (array of short strings where the candidate clearly matches), \"gaps\" " +
        "(array of short strings for requirements the candidate lacks or that are unclear), and \"summary\" " +
        "(one or two sentences of honest overall assessment). Judge strictly from the CANDIDATE PROFILE " +
        "against the JOB DESCRIPTION; never invent qualifications.";

    // The candidate's LaTeX résumé template — the exact format and their baseline content. The LaTeX
    // tailoring keeps this preamble/header verbatim and only reshapes the section content to the role.
    private const string LatexTemplate = @"\documentclass[letterpaper,10pt]{article}

\usepackage{latexsym}
\usepackage[empty]{fullpage}
\usepackage{titlesec}
\usepackage{marvosym}
\usepackage{enumitem}
\usepackage{multicol}
\usepackage[usenames,dvipsnames]{color}
\usepackage{verbatim}
\usepackage{enumitem}
\usepackage[hidelinks]{hyperref}
\usepackage{fancyhdr}
\usepackage[english]{babel}
\usepackage{tabularx}
\usepackage[left=1in,right=1in,top=1in,bottom=1in]{geometry}
\input{glyphtounicode}

% Font
\usepackage[sfdefault]{roboto}  % Sans-serif font

\pagestyle{fancy}
\fancyhf{}
\fancyfoot{}
\renewcommand{\headrulewidth}{0pt}
\renewcommand{\footrulewidth}{0pt}

\pdfgentounicode=1
\urlstyle{same}
\linespread{1.1}
\raggedbottom
\raggedright
\setlength{\tabcolsep}{0in}

% Section formatting
\titleformat{\section}{\Large\bfseries\scshape\raggedright}{}{0em}{}[\titlerule]

% Custom commands
\newcommand{\resumeItem}[1]{\item #1 \vspace{-2pt}}
\newcommand{\resumeSubheading}[4]{
\vspace{1pt}\item
  \begin{tabular*}{0.97\textwidth}[t]{l@{\extracolsep{\fill}}r}
    \textbf{#1} & #2 \\
    \textit{#3} & \textit{#4} \\
  \end{tabular*}\vspace{-5pt}
}
\renewcommand\labelitemii{$\vcenter{\hbox{\tiny$\bullet$}}$}
\newcommand{\resumeSubHeadingList}{\begin{itemize}[leftmargin=0.15in, label={}]}
\newcommand{\resumeSubHeadingListEnd}{\end{itemize}}
% Bulleted list for experience
\newcommand{\resumeBulletList}{\begin{itemize}[leftmargin=0.3in, label={\small\textbullet}]}
\newcommand{\resumeBulletListEnd}{\end{itemize}}

\begin{document}

\begin{center}
  \textbf{\Huge Bayron Ramírez Jiménez} \\
  \vspace{3pt}
  {\large Full-Stack Software Engineer} \\
  \vspace{3pt}
  \small
  Costa Rica \textbar{}
  +506 6002-8152 \textbar{}
  \href{mailto:ramirezjimenezbayron@gmail.com}{ramirezjimenezbayron@gmail.com} \\
  \href{https://linkedin.com/in/bayron-ramirez-jimenez}{linkedin.com/in/bayron-ramirez-jimenez} \textbar{}
  \href{https://github.com/BayronRamirez26}{github.com/BayronRamirez26}
\end{center}

\section{Summary}
% ... tailor to the role ...

\section{Technical Skills}
\resumeSubHeadingList
  % ... \resumeItem lines ...
\resumeSubHeadingListEnd

\section{Experience}
\resumeSubHeadingList
  % ... \resumeSubheading + \resumeBulletList blocks ...
\resumeSubHeadingListEnd

\section{Projects}
\resumeSubHeadingList
  % ... \resumeSubheading + \resumeBulletList blocks ...
\resumeSubHeadingListEnd

\section{Education}
% ... institution and degree ...

\section{Languages}
\resumeSubHeadingList
  % ... \resumeItem lines ...
\resumeSubHeadingListEnd

\end{document}";

    private const string TailoredCvLatexPrompt =
        "You produce a compile-ready LaTeX resume tailored to a specific job. Use the TEMPLATE below as the " +
        "exact format and the candidate's baseline. Reproduce its preamble, header, and every custom command " +
        "definition VERBATIM, and keep the header identity and contact block exactly as written. Tailor ONLY " +
        "the section content (Summary, Technical Skills, Experience, Projects, Education, Languages) to the JOB " +
        "DESCRIPTION: reorder and rephrase bullet points to foreground the most relevant experience, drop or " +
        "de-emphasize weakly relevant items, and mirror the posting's language where it is truthful. Ground " +
        "every claim in the CANDIDATE PROFILE and the template; never fabricate experience, employers, skills, " +
        "or dates. Escape LaTeX special characters (& % $ # _) in generated prose. Return ONLY the LaTeX " +
        "source — no markdown fences, no commentary.\n\nTEMPLATE:\n" + LatexTemplate;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
    };

    private readonly IAiCompletionClient _ai;
    private readonly IValidator<AssistRequest> _validator;

    public AssistantService(IAiCompletionClient ai, IValidator<AssistRequest> validator)
    {
        _ai = ai;
        _validator = validator;
    }

    public async Task<CoverLetterResponse> CoverLetterAsync(AssistRequest request, CancellationToken cancellationToken = default)
    {
        var completion = await CompleteAsync(CoverLetterPrompt, request, cancellationToken);
        return new CoverLetterResponse(completion.Text.Trim(), completion.Model);
    }

    public async Task<TailoredCvResponse> TailoredCvAsync(AssistRequest request, CancellationToken cancellationToken = default)
    {
        var latex = string.Equals(request.Format, "latex", StringComparison.OrdinalIgnoreCase);
        var completion = await CompleteAsync(latex ? TailoredCvLatexPrompt : TailoredCvPrompt, request, cancellationToken);
        return new TailoredCvResponse(StripFences(completion.Text.Trim()), latex ? "latex" : "markdown", completion.Model);
    }

    public async Task<FitResponse> FitAsync(AssistRequest request, CancellationToken cancellationToken = default)
    {
        var completion = await CompleteAsync(FitPrompt, request, cancellationToken);
        var fit = ParseFit(completion.Text);

        return new FitResponse(
            Math.Clamp(fit.Score ?? 0, 0, 100),
            fit.Strengths ?? Array.Empty<string>(),
            fit.Gaps ?? Array.Empty<string>(),
            fit.Summary?.Trim() ?? string.Empty,
            completion.Model);
    }

    private async Task<AiCompletion> CompleteAsync(string systemPrompt, AssistRequest request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var jobDescription = JobDescription.Create(request.JobDescription);

        var userPrompt = string.IsNullOrWhiteSpace(request.CandidateProfile)
            ? jobDescription.Value
            : $"CANDIDATE PROFILE:\n{request.CandidateProfile.Trim()}\n\nJOB DESCRIPTION:\n{jobDescription.Value}";

        return await _ai.CompleteAsync(systemPrompt, userPrompt, cancellationToken);
    }

    /// <summary>Removes a single surrounding triple-backtick code fence if the model added one.</summary>
    private static string StripFences(string text)
    {
        if (!text.StartsWith("```", StringComparison.Ordinal))
        {
            return text;
        }

        var firstNewline = text.IndexOf('\n');
        var body = firstNewline >= 0 ? text[(firstNewline + 1)..] : text;
        if (body.EndsWith("```", StringComparison.Ordinal))
        {
            body = body[..^3];
        }

        return body.Trim();
    }

    private static ParsedFit ParseFit(string text)
    {
        var start = text.IndexOf('{');
        var end = text.LastIndexOf('}');
        if (start < 0 || end <= start)
        {
            throw new AiUnavailableException("The AI did not return a fit assessment.");
        }

        try
        {
            return JsonSerializer.Deserialize<ParsedFit>(text[start..(end + 1)], JsonOptions) ?? EmptyFit;
        }
        catch (JsonException)
        {
            throw new AiUnavailableException("The AI returned an unexpected format. Please try again.");
        }
    }

    private static readonly ParsedFit EmptyFit = new(null, null, null, null);

    private sealed record ParsedFit(
        int? Score,
        IReadOnlyList<string>? Strengths,
        IReadOnlyList<string>? Gaps,
        string? Summary);
}
