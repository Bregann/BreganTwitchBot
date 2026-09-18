export interface ChannelSummary {
  broadcasterChannelName: string
  isLive: boolean
  pointsName: string
  subathonActive: boolean
  trackedViewers: number
  totalStreams: number
}

export interface StreamHistoryItem {
  streamId: number
  streamStarted: string
  streamEnded: string | null
  avgViewCount: number
  peakViewerCount: number
  messagesReceived: number
  newFollowers: number
  newSubscribers: number
  bitsDonated: number
  uniquePeople: number
  uptime: string
}

export interface LeaderboardPosition {
  position: number
  username: string
  value: number
}

export interface Leaderboard {
  leaderboardName: string
  broadcasterChannelName: string
  positions: LeaderboardPosition[]
}

export interface CustomCommand {
  commandName: string
  commandText: string
  timesUsed: number
}

export interface SubathonStatus {
  active: boolean
  secondsLeft: number
  totalTimeAdded: string
  startedAt: string | null
  endsAt: string | null
}

export interface SubathonContributor {
  position: number
  username: string
  amount: number
}

export interface SubathonLeaderboard {
  topBitsDonators: SubathonContributor[]
  topSubGifters: SubathonContributor[]
}

/** Mirrors DiscordLeaderboardType on the api */
export const leaderboardTypes = [
  { value: 'Points', label: 'Points' },
  { value: 'AllTimeHours', label: 'All time hours' },
  { value: 'StreamHours', label: 'This stream' },
  { value: 'WeeklyHours', label: 'This week' },
  { value: 'MonthlyHours', label: 'This month' },
  { value: 'DailyStreak', label: 'Daily streak' },
  { value: 'Marbles', label: 'Marbles wins' },
  { value: 'DiscordLevel', label: 'Discord level' },
  { value: 'DiscordXp', label: 'Discord xp' },
] as const

/** Leaderboards whose values are minutes and should be shown as hours */
export const hourBasedLeaderboards = ['AllTimeHours', 'StreamHours', 'WeeklyHours', 'MonthlyHours']
