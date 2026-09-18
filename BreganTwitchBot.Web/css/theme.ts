import { createTheme } from '@mantine/core'

/**
 * Mantine theme.
 *
 * The dark palette is the same override used in orbit.web, as Mantine's stock
 * dark shades are a little blue. The primary colour is Twitch purple since the
 * whole site hangs off Twitch identity.
 */
export const theme = createTheme({
  primaryColor: 'twitch',
  colors: {
    dark: [
      '#F3F4F6',
      '#DFE2E6',
      '#C3C7CD',
      '#9BA1A9',
      '#6E757E',
      '#4A4F57',
      '#2F333B',
      '#23272E',
      '#1D2026',
      '#15171C'
    ],
    twitch: [
      '#F3EBFF',
      '#E0D1FF',
      '#C7A8FF',
      '#AC7EFF',
      '#9659FF',
      '#9146FF',
      '#7C3AED',
      '#6D28D9',
      '#5B21B6',
      '#4C1D95'
    ]
  }
})
