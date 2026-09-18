export const channelPermissions = [
  { value: 'ViewAdmin', label: 'View admin', description: 'See the admin area at all' },
  { value: 'EditCommands', label: 'Commands', description: 'Add, edit and delete custom commands' },
  { value: 'EditRanks', label: 'Ranks and rewards', description: 'Watchtime ranks and channel point rewards' },
  { value: 'EditSubathon', label: 'Subathon', description: 'Rates, starting and stopping, adding time' },
  { value: 'EditGiveaways', label: 'Giveaways', description: 'Giveaway configuration' },
  { value: 'EditBlacklist', label: 'Word blacklist', description: 'Banned, timeout and warn words' },
  { value: 'EditDiscord', label: 'Discord', description: 'Discord channels, roles and integration' },
  { value: 'EditChannelConfig', label: 'Channel config', description: 'Currency name, point caps and general settings' },
] as const

export interface PermissionHolder {
  twitchUsername: string
  twitchUserId: string
  permissions: string[]
  grantedAt: string
  grantedByTwitchUserId: string
}

export interface SetPermissionsRequest {
  twitchUsername: string
  permissions: string[]
}
