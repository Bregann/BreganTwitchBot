import AdminRewardsComponent from '@/components/pages/channel/admin/rewards/AdminRewardsComponent'

export default async function AdminRewardsPage({ params }: { params: Promise<{ channel: string }> }) {
  const { channel } = await params

  return <AdminRewardsComponent channel={channel} />
}
