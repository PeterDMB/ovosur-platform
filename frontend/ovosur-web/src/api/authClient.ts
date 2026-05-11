import axios from 'axios'

const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5274',
})

export type AuthUser = {
  usuarioId: string
  email: string
  tipoUsuario: string
  estadoAprobacion: string
  roles: string[]
  proveedor?: {
    proveedorId: number
    ruc: string
    razonSocial: string
    estadoHomologacion: string
  } | null
}

export type AuthSession = {
  accessToken: string
  refreshToken: string
  expiresAt: string
  user: AuthUser
}

export type LoginRequest = {
  email: string
  password: string
}

export async function login(request: LoginRequest) {
  const response = await api.post<AuthSession>('/api/auth/login', request)
  return response.data
}

export function getApiError(error: unknown) {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data as { message?: string } | undefined
    return data?.message ?? 'No se pudo conectar con la API.'
  }

  return 'No se pudo procesar la solicitud.'
}
