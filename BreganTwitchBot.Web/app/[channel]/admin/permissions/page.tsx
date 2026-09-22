import AdminPermissionsComponent from '@/components/pages/channel/admin/permissions/AdminPermissionsComponent'

export default async function PermissionsPage({ params }: { params: Promise<{ channel: string }> }) {
  const { channel } = await params

  return <AdminPermissionsComponent channel={channel} />
}
