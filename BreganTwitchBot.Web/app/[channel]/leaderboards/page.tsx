import ChannelLeaderboardsComponent from '@/components/pages/channel/leaderboards/ChannelLeaderboardsComponent'

export default async function ChannelLeaderboardsPage({ params }: { params: Promise<{ channel: string }> }) {
  const { channel } = await params

  return <ChannelLeaderboardsComponent channel={channel} />
}
