/** Turn a PascalCase enum value ("PhoneScreen") into a readable label ("Phone Screen"). */
export function humanize(value: string): string {
  // Brand names that aren't really two words.
  const exceptions: Record<string, string> = { LinkedIn: 'LinkedIn' };
  return exceptions[value] ?? value.replace(/([a-z])([A-Z])/g, '$1 $2');
}
