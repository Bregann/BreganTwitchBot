export const runtime = 'nodejs'

import { NextRequest, NextResponse } from 'next/server'
import { cookies } from 'next/headers'

// the .NET api. In development it's the local https endpoint
let API_BASE_URL = process.env.API_BASE_URL

if (process.env.NODE_ENV === 'development') {
  API_BASE_URL = 'https://localhost:7248/api'
}

const ACCESS_TOKEN_COOKIE = 'accessToken'

// headers that must not be forwarded from the browser to the backend
const HOP_BY_HOP_HEADERS = [
  'host',
  'connection',
  'keep-alive',
  'proxy-authenticate',
  'proxy-authorization',
  'te',
  'trailers',
  'transfer-encoding',
  'upgrade',
  'content-length',
]

/**
 * Backend for frontend proxy.
 *
 * The browser never sees the access token - it lives in an httpOnly cookie
 * which this route reads and turns into an Authorization header. That keeps
 * the token out of reach of any script on the page.
 */
async function handler(
  req: NextRequest,
  { params }: { params: Promise<{ route: string[] }> }
) {
  const { route } = await params
  const queryString = req.nextUrl.searchParams.toString()
  const url = `${API_BASE_URL}/${route.join('/')}${queryString !== '' ? `?${queryString}` : ''}`

  const headers = new Headers()

  req.headers.forEach((value, key) => {
    if (!HOP_BY_HOP_HEADERS.includes(key.toLowerCase())) {
      headers.set(key, value)
    }
  })

  const cookieStore = await cookies()
  const accessToken = cookieStore.get(ACCESS_TOKEN_COOKIE)?.value

  if (accessToken !== undefined) {
    headers.set('Authorization', `Bearer ${accessToken}`)
  }

  const hasBody = !['GET', 'HEAD'].includes(req.method)

  try {
    const res = await fetch(url, {
      method: req.method,
      headers,
      body: hasBody ? await req.text() : undefined,
      redirect: 'manual',
    })

    const responseHeaders = new Headers()

    res.headers.forEach((value, key) => {
      if (!HOP_BY_HOP_HEADERS.includes(key.toLowerCase())) {
        responseHeaders.set(key, value)
      }
    })

    return new NextResponse(res.body, {
      status: res.status,
      headers: responseHeaders,
    })
  } catch (error) {
    console.error(`[proxy] ${req.method} ${url} failed`, error)

    return NextResponse.json(
      { title: 'The API could not be reached' },
      { status: 502 }
    )
  }
}

export {
  handler as GET,
  handler as POST,
  handler as PUT,
  handler as PATCH,
  handler as DELETE,
}
