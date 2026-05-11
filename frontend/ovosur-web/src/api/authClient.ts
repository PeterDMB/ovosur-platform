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

export type SupplierApproval = {
  proveedorId: number
  usuarioId: string
  ruc: string
  razonSocial: string
  email: string
  estadoAprobacion: string
  estadoHomologacion: string
  fechaRegistro: string
}

export async function login(request: LoginRequest) {
  const response = await api.post<AuthSession>('/api/auth/login', request)
  return response.data
}

export async function listSuppliers(session: AuthSession, status = 'PENDIENTE') {
  const response = await api.get<SupplierApproval[]>('/api/suppliers', {
    params: { status },
    headers: { Authorization: `Bearer ${session.accessToken}` },
  })
  return response.data
}

export async function updateSupplierApproval(
  session: AuthSession,
  proveedorId: number,
  decision: 'APROBAR' | 'OBSERVAR' | 'RECHAZAR',
) {
  const response = await api.patch(
    `/api/suppliers/${proveedorId}/approval`,
    { decision },
    { headers: { Authorization: `Bearer ${session.accessToken}` } },
  )
  return response.data
}

export function getApiError(error: unknown) {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data as { message?: string } | undefined
    return data?.message ?? 'No se pudo conectar con la API.'
  }

  return 'No se pudo procesar la solicitud.'
}
