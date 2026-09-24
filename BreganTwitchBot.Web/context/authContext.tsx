'use client'

import { doGet, doPost } from '@/helpers/apiClient'
import { CurrentUser } from '@/interfaces/api/auth/CurrentUser'
import { useRouter } from 'next/navigation'
import { createContext, useCallback, useContext, useEffect, useState } from 'react'

type AuthContextType = {
  isAuthenticated: boolean
  isLoading: boolean
  user: CurrentUser | null
  logout: () => Promise<void>
  refresh: () => Promise<void>
}

const AuthContext = createContext<AuthContextType | undefined>(undefined)

/**
 * Tracks who is signed in.
 *
 * The tokens live in httpOnly cookies that this code deliberately cannot read,
 * so identity comes from asking the api rather than decoding a token here. The
 * api client handles refreshing on a 401, so this only needs to fetch once and
 * whenever something asks it to.
 */
export const AuthProvider = ({ children }: { children: React.ReactNode }) => {
  const [user, setUser] = useState<CurrentUser | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const router = useRouter()

  const loadUser = useCallback(async () => {
    const res = await doGet<CurrentUser>('/api/Auth/Me')

    setUser(res.ok && res.data !== undefined ? res.data : null)
    setIsLoading(false)
  }, [])

  useEffect(() => {
    void loadUser()
  }, [loadUser])

  const logout = async () => {
    await doPost('/api/Auth/Logout', {})
    setUser(null)
    router.push('/')
  }

  return (
    <AuthContext.Provider
      value={{
        isAuthenticated: user !== null,
        isLoading,
        user,
        logout,
        refresh: loadUser
      }}
    >
      {children}
    </AuthContext.Provider>
  )
}

export const useAuth = (): AuthContextType => {
  const context = useContext(AuthContext)

  if (context === undefined) {
    throw new Error('useAuth must be used inside an AuthProvider')
  }

  return context
}
