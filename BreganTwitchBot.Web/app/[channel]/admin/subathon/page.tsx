import AdminSubathonComponent from '@/components/pages/channel/admin/subathon/AdminSubathonComponent'

export default async function AdminSubathonPage({ params }: { params: Promise<{ channel: string }> }) {
  const { channel } = await params

  return <AdminSubathonComponent channel={channel} />
}
