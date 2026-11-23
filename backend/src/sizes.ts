// Helpers shared by the seed script and the API for working with size labels.
// They mirror the rules used in the normalize_sizes migration.

const ALPHA_ORDER = ["XXS", "XS", "S", "M", "L", "XL", "XXL", "XXXL"];

export type SizeSystemName = "ALPHA" | "WAIST";

/** Normalizes one size label: trims spaces and upper-cases letters. */
export function normalizeSizeLabel(label: string) {
  return label.trim().toUpperCase();
}

/** Splits a list such as "S, M, L" into unique, normalized labels. */
export function parseSizeList(value: string | undefined | null) {
  if (!value) return [];
  const labels = value.split(",").map(normalizeSizeLabel).filter((label) => label.length > 0);
  return [...new Set(labels)];
}

/** True for labels the showroom understands: letter sizes or a 2-digit waist size. */
export function isValidSizeLabel(label: string) {
  return ALPHA_ORDER.includes(label) || /^[0-9]{2}$/.test(label);
}

export function sizeSystemFor(label: string): SizeSystemName {
  return /^[0-9]+$/.test(label) ? "WAIST" : "ALPHA";
}

export function sortOrderFor(label: string) {
  if (/^[0-9]+$/.test(label)) return Number(label);
  const position = ALPHA_ORDER.indexOf(label);
  return position === -1 ? 99 : position + 1;
}
