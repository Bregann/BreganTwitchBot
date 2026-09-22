import ChannelOverviewComponent from '@/components/pages/channel/ChannelOverviewComponent'

export default async function ChannelOverviewPage({ params }: { params: Promise<{ channel: string }> }) {
  const { channel } = await params

  return <ChannelOverviewComponent channel={channel} />
}
