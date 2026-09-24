import ChannelCommandsComponent from '@/components/pages/channel/commands/ChannelCommandsComponent'

export default async function ChannelCommandsPage({ params }: { params: Promise<{ channel: string }> }) {
  const { channel } = await params

  return <ChannelCommandsComponent channel={channel} />
}
