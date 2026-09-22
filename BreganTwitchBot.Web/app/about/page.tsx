import AboutComponent from '@/components/pages/about/AboutComponent'
import type { Metadata } from 'next'

export const metadata: Metadata = {
  title: 'About'
}

export default function AboutPage() {
  return <AboutComponent />
}
