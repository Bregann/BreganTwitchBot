import AdminSettingsComponent from '@/components/pages/channel/admin/settings/AdminSettingsComponent'

export default async function AdminSettingsPage({ params }: { params: Promise<{ channel: string }> }) {
  const { channel } = await params

  return <AdminSettingsComponent channel={channel} />
}
