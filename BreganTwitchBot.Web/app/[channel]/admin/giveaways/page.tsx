import AdminGiveawaysComponent from '@/components/pages/channel/admin/giveaways/AdminGiveawaysComponent'

export default async function AdminGiveawaysPage({ params }: { params: Promise<{ channel: string }> }) {
  const { channel } = await params

  return <AdminGiveawaysComponent channel={channel} />
}
