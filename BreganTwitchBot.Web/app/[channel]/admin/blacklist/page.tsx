import AdminBlacklistComponent from '@/components/pages/channel/admin/blacklist/AdminBlacklistComponent'

export default async function AdminBlacklistPage({ params }: { params: Promise<{ channel: string }> }) {
  const { channel } = await params

  return <AdminBlacklistComponent channel={channel} />
}
