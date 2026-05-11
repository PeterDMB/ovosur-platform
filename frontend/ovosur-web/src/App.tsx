import { type FormEvent, useEffect, useMemo, useState } from 'react'
import {
  AlertTriangle,
  Building2,
  CheckCircle2,
  ClipboardList,
  Clock3,
  FileText,
  KeyRound,
  LayoutDashboard,
  PackageCheck,
  RefreshCw,
  ShieldCheck,
  Truck,
  UserPlus,
  WalletCards,
  XCircle,
} from 'lucide-react'
import {
  getApiError,
  listSuppliers,
  login,
  registerProvider,
  updateSupplierApproval,
  type AuthSession,
  type SupplierApproval,
} from './api/authClient'
import './App.css'

type ActiveView = 'dashboard' | 'proveedores' | 'ventas' | 'tesoreria' | 'distribucion' | 'seguridad'
type PublicView = 'proveedor-login' | 'colaborador-login' | 'registro-proveedor'

const modules = [
  { code: 'proveedores', name: 'Proveedores', status: 'Activo', icon: Building2 },
  { code: 'ventas', name: 'Ventas', status: 'Proximo modulo', icon: PackageCheck },
  { code: 'tesoreria', name: 'Tesoreria', status: 'Proximo modulo', icon: WalletCards },
  { code: 'distribucion', name: 'Distribucion', status: 'Proximo modulo', icon: Truck },
  { code: 'seguridad', name: 'Seguridad', status: 'Base activa', icon: ShieldCheck },
] as const

const supplierStatuses = ['PENDIENTE', 'OBSERVADO', 'APROBADO', 'RECHAZADO']

function App() {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [publicView, setPublicView] = useState<PublicView>('proveedor-login')
  const [activeView, setActiveView] = useState<ActiveView>('dashboard')
  const [session, setSession] = useState<AuthSession | null>(() => {
    const raw = window.localStorage.getItem('ovosur.session')
    return raw ? (JSON.parse(raw) as AuthSession) : null
  })
  const [message, setMessage] = useState<string | null>(null)
  const [successMessage, setSuccessMessage] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(false)
  const [suppliers, setSuppliers] = useState<SupplierApproval[]>([])
  const [supplierStatus, setSupplierStatus] = useState('PENDIENTE')
  const [supplierMessage, setSupplierMessage] = useState<string | null>(null)
  const [isLoadingSuppliers, setIsLoadingSuppliers] = useState(false)

  const [providerForm, setProviderForm] = useState({
    ruc: '',
    razonSocial: '',
    nombreComercial: '',
    email: '',
    password: '',
    direccionFiscal: '',
    telefono: '',
  })

  const isAdmin = session?.user.roles.includes('SUPER_ADMIN') ?? false
  const pageTitle = useMemo(() => {
    if (activeView === 'dashboard') return 'Panel principal'
    const module = modules.find((item) => item.code === activeView)
    return module ? module.name : 'Panel principal'
  }, [activeView])

  useEffect(() => {
    if (session && isAdmin && activeView === 'proveedores') {
      void handleLoadSuppliers()
    }
  }, [session, isAdmin, supplierStatus, activeView])

  async function handleLogin(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setMessage(null)
    setSuccessMessage(null)
    setIsLoading(true)

    try {
      const nextSession = await login({ email, password })
      window.localStorage.setItem('ovosur.session', JSON.stringify(nextSession))
      setSession(nextSession)
      setActiveView('dashboard')
      setPassword('')
    } catch (error) {
      setMessage(getApiError(error))
    } finally {
      setIsLoading(false)
    }
  }

  async function handleProviderRegister(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setMessage(null)
    setSuccessMessage(null)
    setIsLoading(true)

    try {
      await registerProvider(providerForm)
      setSuccessMessage('Proveedor registrado. Queda pendiente de aprobacion por Compras.')
      setPublicView('proveedor-login')
      setProviderForm({
        ruc: '',
        razonSocial: '',
        nombreComercial: '',
        email: '',
        password: '',
        direccionFiscal: '',
        telefono: '',
      })
    } catch (error) {
      setMessage(getApiError(error))
    } finally {
      setIsLoading(false)
    }
  }

  function handleLogout() {
    window.localStorage.removeItem('ovosur.session')
    setSession(null)
    setSuppliers([])
    setActiveView('dashboard')
    setPublicView('proveedor-login')
  }

  async function handleLoadSuppliers() {
    if (!session) return
    setSupplierMessage(null)
    setIsLoadingSuppliers(true)

    try {
      const data = await listSuppliers(session, supplierStatus)
      setSuppliers(data)
    } catch (error) {
      setSupplierMessage(getApiError(error))
    } finally {
      setIsLoadingSuppliers(false)
    }
  }

  async function handleSupplierDecision(
    proveedorId: number,
    decision: 'APROBAR' | 'OBSERVAR' | 'RECHAZAR',
  ) {
    if (!session) return
    setSupplierMessage(null)

    try {
      await updateSupplierApproval(session, proveedorId, decision)
      await handleLoadSuppliers()
    } catch (error) {
      setSupplierMessage(getApiError(error))
    }
  }

  function updateProviderField(field: keyof typeof providerForm, value: string) {
    setProviderForm((current) => ({ ...current, [field]: value }))
  }

  if (!session) {
    return (
      <main className="public-shell">
        <section className={`auth-card ${publicView === 'registro-proveedor' ? 'wide' : ''}`}>
          <div className="auth-brand">
            <div className="brand-mark">OV</div>
            <div>
              <strong>OVOSUR</strong>
              <span>Intranet / Extranet</span>
            </div>
          </div>

          {publicView === 'registro-proveedor' ? (
            <form onSubmit={handleProviderRegister}>
              <div className="auth-heading">
                <h1>Registro de proveedor</h1>
                <p>Completa la solicitud para evaluacion interna.</p>
              </div>

              <div className="form-grid">
                <label>
                  RUC
                  <input
                    maxLength={11}
                    value={providerForm.ruc}
                    onChange={(event) => updateProviderField('ruc', event.target.value)}
                  />
                </label>
                <label>
                  Razon social
                  <input
                    value={providerForm.razonSocial}
                    onChange={(event) => updateProviderField('razonSocial', event.target.value)}
                  />
                </label>
                <label>
                  Nombre comercial
                  <input
                    value={providerForm.nombreComercial}
                    onChange={(event) => updateProviderField('nombreComercial', event.target.value)}
                  />
                </label>
                <label>
                  Correo
                  <input
                    type="email"
                    value={providerForm.email}
                    onChange={(event) => updateProviderField('email', event.target.value)}
                  />
                </label>
                <label>
                  Contrasena
                  <input
                    type="password"
                    value={providerForm.password}
                    onChange={(event) => updateProviderField('password', event.target.value)}
                  />
                </label>
                <label>
                  Telefono
                  <input
                    value={providerForm.telefono}
                    onChange={(event) => updateProviderField('telefono', event.target.value)}
                  />
                </label>
              </div>

              <label>
                Direccion fiscal
                <input
                  value={providerForm.direccionFiscal}
                  onChange={(event) => updateProviderField('direccionFiscal', event.target.value)}
                />
              </label>

              {message ? <p className="alert error">{message}</p> : null}
              {successMessage ? <p className="alert success">{successMessage}</p> : null}

              <button className="primary-action" type="submit" disabled={isLoading}>
                <UserPlus size={18} />
                {isLoading ? 'Registrando...' : 'Enviar solicitud'}
              </button>

              <button className="text-action" type="button" onClick={() => setPublicView('proveedor-login')}>
                Volver al ingreso de proveedores
              </button>
            </form>
          ) : (
            <form onSubmit={handleLogin}>
              <div className="auth-heading">
                <h1>{publicView === 'colaborador-login' ? 'Colaboradores OVOSUR' : 'Portal de proveedores'}</h1>
                <p>
                  {publicView === 'colaborador-login'
                    ? 'Acceso exclusivo para personal interno.'
                    : 'Ingresa con tu cuenta aprobada.'}
                </p>
              </div>

              <label>
                Correo
                <input
                  type="email"
                  placeholder={publicView === 'colaborador-login' ? 'usuario@ovosur.com' : 'proveedor@empresa.com'}
                  value={email}
                  onChange={(event) => setEmail(event.target.value)}
                />
              </label>

              <label>
                Contrasena
                <input
                  type="password"
                  placeholder="Ingrese su contrasena"
                  value={password}
                  onChange={(event) => setPassword(event.target.value)}
                />
              </label>

              {message ? <p className="alert error">{message}</p> : null}
              {successMessage ? <p className="alert success">{successMessage}</p> : null}

              <button className="primary-action" type="submit" disabled={isLoading}>
                <KeyRound size={18} />
                {isLoading ? 'Validando...' : 'Ingresar'}
              </button>

              {publicView === 'proveedor-login' ? (
                <>
                  <button className="outline-action" type="button" onClick={() => setPublicView('registro-proveedor')}>
                    Registro de proveedor
                  </button>
                  <button className="text-action" type="button" onClick={() => setPublicView('colaborador-login')}>
                    Ingresar como colaborador OVOSUR
                  </button>
                </>
              ) : (
                <button className="text-action" type="button" onClick={() => setPublicView('proveedor-login')}>
                  Volver al portal de proveedores
                </button>
              )}
            </form>
          )}
        </section>
      </main>
    )
  }

  return (
    <main className="app-shell">
      <aside className="sidebar" aria-label="Navegacion principal">
        <div className="brand">
          <div className="brand-mark">OV</div>
          <div>
            <span>OVOSUR</span>
            <small>Platform</small>
          </div>
        </div>

        <nav className="nav-list">
          <button
            className={`nav-item ${activeView === 'dashboard' ? 'active' : ''}`}
            type="button"
            onClick={() => setActiveView('dashboard')}
          >
            <LayoutDashboard size={18} />
            Consola
          </button>
          <button
            className={`nav-item ${activeView === 'proveedores' ? 'active' : ''}`}
            type="button"
            onClick={() => setActiveView('proveedores')}
            disabled={!isAdmin && session.user.tipoUsuario !== 'PROVEEDOR'}
          >
            <Building2 size={18} />
            Proveedores
          </button>
          <button
            className={`nav-item ${activeView === 'ventas' ? 'active' : ''}`}
            type="button"
            onClick={() => setActiveView('ventas')}
          >
            <PackageCheck size={18} />
            Ventas
          </button>
          <button
            className={`nav-item ${activeView === 'seguridad' ? 'active' : ''}`}
            type="button"
            onClick={() => setActiveView('seguridad')}
            disabled={!isAdmin}
          >
            <ShieldCheck size={18} />
            Seguridad
          </button>
        </nav>
      </aside>

      <section className="workspace">
        <header className="topbar">
          <div>
            <p className="eyebrow">Intranet / Extranet</p>
            <h1>{pageTitle}</h1>
          </div>
          <span className="environment">Desarrollo local</span>
        </header>

        {activeView === 'dashboard' ? (
          <DashboardView session={session} isAdmin={isAdmin} onLogout={handleLogout} onOpenSuppliers={() => setActiveView('proveedores')} />
        ) : null}

        {activeView === 'proveedores' ? (
          isAdmin ? (
            <AdminSuppliersView
              suppliers={suppliers}
              supplierStatus={supplierStatus}
              supplierMessage={supplierMessage}
              isLoadingSuppliers={isLoadingSuppliers}
              onStatusChange={setSupplierStatus}
              onLoadSuppliers={handleLoadSuppliers}
              onDecision={handleSupplierDecision}
            />
          ) : (
            <SupplierPortalView session={session} />
          )
        ) : null}

        {activeView !== 'dashboard' && activeView !== 'proveedores' ? (
          <ModulePlaceholder activeView={activeView} />
        ) : null}
      </section>
    </main>
  )
}

function DashboardView({
  session,
  isAdmin,
  onLogout,
  onOpenSuppliers,
}: {
  session: AuthSession
  isAdmin: boolean
  onLogout: () => void
  onOpenSuppliers: () => void
}) {
  return (
    <section className="dashboard-grid wide">
      <section className="welcome-panel">
        <div className="panel-title">
          <ShieldCheck size={22} />
          <div>
            <h2>Sesion activa</h2>
            <p>{session.user.email}</p>
          </div>
        </div>
        <div className="metric-row">
          <span>Tipo</span>
          <strong>{session.user.tipoUsuario}</strong>
        </div>
        <div className="metric-row">
          <span>Estado</span>
          <strong>{session.user.estadoAprobacion}</strong>
        </div>
        <div className="metric-row">
          <span>Roles</span>
          <strong>{session.user.roles.join(', ')}</strong>
        </div>
        <button className="secondary-action" type="button" onClick={onLogout}>
          Cerrar sesion
        </button>
      </section>

      <section className="module-panel">
        <div className="panel-title">
          <LayoutDashboard size={22} />
          <div>
            <h2>Modulos corporativos</h2>
            <p>{isAdmin ? 'Administracion interna' : 'Portal proveedor'}</p>
          </div>
        </div>

        <div className="module-grid">
          {modules.map((module) => {
            const Icon = module.icon
            const enabled = module.code === 'proveedores' || module.code === 'seguridad'
            return (
              <article className={`module-card ${enabled ? 'enabled' : ''}`} key={module.code}>
                <Icon size={24} />
                <div>
                  <strong>{module.name}</strong>
                  <span>{module.status}</span>
                </div>
                {module.code === 'proveedores' ? (
                  <button type="button" onClick={onOpenSuppliers}>
                    Abrir
                  </button>
                ) : null}
              </article>
            )
          })}
        </div>
      </section>
    </section>
  )
}

function AdminSuppliersView({
  suppliers,
  supplierStatus,
  supplierMessage,
  isLoadingSuppliers,
  onStatusChange,
  onLoadSuppliers,
  onDecision,
}: {
  suppliers: SupplierApproval[]
  supplierStatus: string
  supplierMessage: string | null
  isLoadingSuppliers: boolean
  onStatusChange: (status: string) => void
  onLoadSuppliers: () => void
  onDecision: (proveedorId: number, decision: 'APROBAR' | 'OBSERVAR' | 'RECHAZAR') => void
}) {
  return (
    <section className="approval-panel full-width">
      <div className="panel-title between">
        <div className="title-inline">
          <Building2 size={22} />
          <div>
            <h2>Aprobacion de proveedores</h2>
            <p>Solicitudes registradas en el portal externo</p>
          </div>
        </div>
        <button className="icon-action" type="button" onClick={onLoadSuppliers} aria-label="Actualizar proveedores">
          <RefreshCw size={18} />
        </button>
      </div>

      <div className="filter-row" role="group" aria-label="Filtro de proveedores">
        {supplierStatuses.map((status) => (
          <button
            key={status}
            type="button"
            className={supplierStatus === status ? 'selected' : ''}
            onClick={() => onStatusChange(status)}
          >
            {status}
          </button>
        ))}
      </div>

      {supplierMessage ? <p className="alert error">{supplierMessage}</p> : null}
      {isLoadingSuppliers ? <p className="muted">Cargando proveedores...</p> : null}

      <div className="supplier-list">
        {suppliers.length === 0 && !isLoadingSuppliers ? (
          <div className="empty-state">
            <AlertTriangle size={22} />
            <span>No hay proveedores en este estado.</span>
          </div>
        ) : null}

        {suppliers.map((supplier) => (
          <article className="supplier-row" key={supplier.proveedorId}>
            <div>
              <strong>{supplier.razonSocial}</strong>
              <span>
                RUC {supplier.ruc} - {supplier.email}
              </span>
            </div>
            <span className="status-pill">{supplier.estadoHomologacion}</span>
            <div className="decision-actions">
              <button type="button" className="approve" onClick={() => onDecision(supplier.proveedorId, 'APROBAR')}>
                <CheckCircle2 size={16} />
                Aprobar
              </button>
              <button type="button" className="observe" onClick={() => onDecision(supplier.proveedorId, 'OBSERVAR')}>
                <AlertTriangle size={16} />
                Observar
              </button>
              <button type="button" className="reject" onClick={() => onDecision(supplier.proveedorId, 'RECHAZAR')}>
                <XCircle size={16} />
                Rechazar
              </button>
            </div>
          </article>
        ))}
      </div>
    </section>
  )
}

function SupplierPortalView({ session }: { session: AuthSession }) {
  return (
    <section className="approval-panel full-width">
      <div className="panel-title">
        <Building2 size={22} />
        <div>
          <h2>Portal proveedor</h2>
          <p>Estado de registro y homologacion</p>
        </div>
      </div>
      <div className="details-grid">
        <div className="metric-row">
          <span>Empresa</span>
          <strong>{session.user.proveedor?.razonSocial ?? 'Pendiente de datos'}</strong>
        </div>
        <div className="metric-row">
          <span>RUC</span>
          <strong>{session.user.proveedor?.ruc ?? 'No registrado'}</strong>
        </div>
        <div className="metric-row">
          <span>Homologacion</span>
          <strong>{session.user.proveedor?.estadoHomologacion ?? 'EN_REVISION'}</strong>
        </div>
      </div>
    </section>
  )
}

function ModulePlaceholder({ activeView }: { activeView: ActiveView }) {
  const module = modules.find((item) => item.code === activeView)
  const Icon = module?.icon ?? ClipboardList

  return (
    <section className="approval-panel full-width">
      <div className="panel-title">
        <Icon size={22} />
        <div>
          <h2>{module?.name ?? 'Modulo'}</h2>
          <p>{module?.status ?? 'Proximo modulo'}</p>
        </div>
      </div>
      <div className="placeholder-grid">
        <article>
          <Clock3 size={22} />
          <strong>Planificado</strong>
          <span>La base de seguridad ya permite agregar permisos y vistas por modulo.</span>
        </article>
        <article>
          <FileText size={22} />
          <strong>Siguiente construccion</strong>
          <span>Se agregaran formularios, tablas, aprobaciones y conexion ERP segun prioridad.</span>
        </article>
      </div>
    </section>
  )
}

export default App
