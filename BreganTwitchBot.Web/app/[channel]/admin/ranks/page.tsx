import AdminRanksComponent from '@/components/pages/channel/admin/ranks/AdminRanksComponent'

export default async function AdminRanksPage({ params }: { params: Promise<{ channel: string }> }) {
  const { channel } = await params

  return <AdminRanksComponent channel={channel} />
}
