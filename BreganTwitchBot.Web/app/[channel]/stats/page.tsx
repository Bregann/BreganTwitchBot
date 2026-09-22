import ChannelStatsComponent from '@/components/pages/channel/stats/ChannelStatsComponent'

export default async function ChannelStatsPage({ params }: { params: Promise<{ channel: string }> }) {
  const { channel } = await params

  return <ChannelStatsComponent channel={channel} />
}
