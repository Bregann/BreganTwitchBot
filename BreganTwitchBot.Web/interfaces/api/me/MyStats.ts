export interface MyChannelStats {
  broadcasterChannelName: string
  pointsName: string
  points: number
  minutesInStream: number
  minutesWatchedThisWeek: number
  minutesWatchedThisMonth: number
  totalMessages: number
  marblesWins: number
  currentRank: string | null
  nextRank: string | null
  minutesUntilNextRank: number | null
  currentDailyStreak: number
  highestDailyStreak: number
  pointsWon: number
  pointsLost: number
  discordLevel: number | null
  discordXp: number | null
}

export interface MyStats {
  twitchUsername: string
  channels: MyChannelStats[]
}

export interface MyChannelSetting {
  broadcasterChannelName: string
  discordLevelUpNotifsEnabled: boolean
}

export interface MySettings {
  channels: MyChannelSetting[]
}
