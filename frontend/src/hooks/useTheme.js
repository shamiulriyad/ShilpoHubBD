import { useTheme as useThemeContext } from '../contexts/ThemeContext';

export function useTheme() {
  return useThemeContext();
}
