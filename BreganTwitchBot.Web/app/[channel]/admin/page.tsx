import AdminHomeComponent from '@/components/pages/channel/admin/AdminHomeComponent'

export default async function AdminHomePage({ params }: { params: Promise<{ channel: string }> }) {
  const { channel } = await params

  return <AdminHomeComponent channel={channel} />
}
