export interface CurrentUser {
  twitchUserId: string
  twitchUsername: string
  twitchDisplayName: string | null
  profileImageUrl: string | null
  /** Channels this user broadcasts, so the UI knows where to offer admin */
  broadcasterOfChannels: string[]
}
