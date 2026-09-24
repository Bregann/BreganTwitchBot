import ChannelSubathonComponent from '@/components/pages/channel/subathon/ChannelSubathonComponent'

export default async function ChannelSubathonPage({ params }: { params: Promise<{ channel: string }> }) {
  const { channel } = await params

  return <ChannelSubathonComponent channel={channel} />
}
