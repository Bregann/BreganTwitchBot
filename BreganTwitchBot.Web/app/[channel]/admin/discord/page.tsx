import AdminDiscordComponent from '@/components/pages/channel/admin/discord/AdminDiscordComponent'

export default async function AdminDiscordPage({ params }: { params: Promise<{ channel: string }> }) {
  const { channel } = await params

  return <AdminDiscordComponent channel={channel} />
}
