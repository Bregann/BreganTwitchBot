import AdminCommandsComponent from '@/components/pages/channel/admin/commands/AdminCommandsComponent'

export default async function AdminCommandsPage({ params }: { params: Promise<{ channel: string }> }) {
  const { channel } = await params

  return <AdminCommandsComponent channel={channel} />
}
