export interface ChannelConfig {
  channelCurrencyName: string
  currencyPointCap: number
  discordEnabled: boolean
}

export interface ChannelRank {
  id: number
  rankName: string
  rankMinutesRequired: number
  bonusRankPointsEarned: number
  discordRoleId: number | null
}

export interface BlacklistWord {
  id: number
  word: string
  wordType: WordType
}

export type WordType = 'PermBanWord' | 'TempBanWord' | 'StrikeWord'

export const wordTypes: { value: WordType, label: string, description: string }[] = [
  { value: 'StrikeWord', label: 'Warn', description: 'Warns the user' },
  { value: 'TempBanWord', label: 'Timeout', description: 'Times the user out' },
  { value: 'PermBanWord', label: 'Ban', description: 'Permanently bans the user' },
]

export interface DiscordConfig {
  discordEnabled: boolean
  discordGuildId: number | null
  discordEventChannelId: number | null
  discordStreamAnnouncementChannelId: number | null
  discordUserCommandsChannelId: number | null
  discordUserRankUpAnnouncementChannelId: number | null
  discordGiveawayChannelId: number | null
  discordGeneralChannelId: number | null
  discordModeratorRoleId: number | null
  discordWelcomeMessageChannelId: number | null
}
