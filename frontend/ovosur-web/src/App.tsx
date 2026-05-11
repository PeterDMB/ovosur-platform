import { type FormEvent, useEffect, useState } from 'react'
import {
  AlertTriangle,
  BadgeCheck,
  Building2,
  CheckCircle2,
  KeyRound,
  LayoutDashboard,
  LockKeyhole,
  PackageCheck,
  RefreshCw,
  ShieldCheck,
  Truck,
  XCircle,
} from 'lucide-react'
import {
  getApiError,
  listSuppliers,
  login,
  updateSupplierApproval,
  type AuthSession,
  type SupplierApproval,
} from './api/authClient'
import './App.css'

const modules = [
  { name: 'Proveedores', status: 'En diseno', icon: Building2 },
  { name: 'Ventas', status: 'Pendiente API', icon: PackageCheck },
  { name: 'Tesoreria', status: 'Pendiente API', icon: ShieldCheck },
  { name: 'Distribucion', status: 'Pendiente API', icon: Truck },
]

function App() {
  const [email, setEmail] = useState('admin@ovosur.local')
  const [password, setPassword] = useState('')
  const [accessType, setAccessType] = useState<'interno' | 'proveedor'>('interno')
  const [session, setSession] = useState<AuthSession | null>(() => {
    const raw = window.localStorage.getItem('ovosur.session')
    return raw ? (JSON.parse(raw) as AuthSession) : null
  })
  const [message, setMessage] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(false)
  const [suppliers, setSuppliers] = useState<SupplierApproval[]>([])
  const [supplierStatus, setSupplierStatus] = useState('PENDIENTE')
  const [supplierMessage, setSupplierMessage] = useState<string | null>(null)
  const [isLoadingSuppliers, setIsLoadingSuppliers] = useState(false)

  const isAdmin = session?.user.roles.includes('SUPER_ADMIN') ?? false

  useEffect(() => {
    if (session && isAdmin) {
      void handleLoadSuppliers()
    }
  }, [session, isAdmin, supplierStatus])

  async function handleLogin(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setMessage(null)
    setIsLoading(true)

    try {
      const nextSession = await login({ email, password })
      window.localStorage.setItem('ovosur.session', JSON.stringify(nextSession))
      setSession(nextSession)
      setPassword('')
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
          <a className="nav-item active" href="/">
            <LayoutDashboard size={18} />
            Consola
          </a>
          <a className="nav-item" href="/proveedores">
            <Building2 size={18} />
            Proveedores
          </a>
          <a className="nav-item" href="/seguridad">
            <ShieldCheck size={18} />
            Seguridad
          </a>
        </nav>
      </aside>

      <section className="workspace">
        <header className="topbar">
          <div>
            <p className="eyebrow">Intranet / Extranet</p>
            <h1>Consola corporativa OVOSUR</h1>
          </div>
          <span className="environment">Desarrollo local</span>
        </header>

        {!session ? (
          <section className="content-grid">
          <form className="login-panel" onSubmit={handleLogin}>
            <div className="panel-title">
              <LockKeyhole size={22} />
              <div>
                <h2>Acceso seguro</h2>
                <p>Usuarios internos y proveedores aprobados</p>
              </div>
            </div>

            <label>
              Correo corporativo
              <input
                type="email"
                placeholder="usuario@ovosur.com"
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

            <div className="segment" role="group" aria-label="Tipo de acceso">
              <button
                type="button"
                className={accessType === 'interno' ? 'selected' : ''}
                onClick={() => setAccessType('interno')}
              >
                Interno
              </button>
              <button
                type="button"
                className={accessType === 'proveedor' ? 'selected' : ''}
                onClick={() => setAccessType('proveedor')}
              >
                Proveedor
              </button>
            </div>

            {message ? <p className="alert error">{message}</p> : null}

            <button className="primary-action" type="submit" disabled={isLoading}>
              <KeyRound size={18} />
              {isLoading ? 'Validando...' : 'Ingresar'}
            </button>
          </form>

          <section className="module-panel" aria-label="Modulos iniciales">
            <div className="panel-title">
              <BadgeCheck size={22} />
              <div>
                <h2>Modulos base</h2>
                <p>Arquitectura lista para crecer por areas</p>
              </div>
            </div>

            <div className="module-list">
              {modules.map((module) => {
                const Icon = module.icon
                return (
                  <article className="module-row" key={module.name}>
                    <Icon size={22} />
                    <div>
                      <strong>{module.name}</strong>
                      <span>{module.status}</span>
                    </div>
                  </article>
                )
              })}
            </div>
          </section>
        </section>
        ) : (
          <section className="dashboard-grid">
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
              <button className="secondary-action" type="button" onClick={handleLogout}>
                Cerrar sesion
              </button>
            </section>

            {isAdmin ? (
              <section className="approval-panel">
                <div className="panel-title between">
                  <div className="title-inline">
                    <Building2 size={22} />
                    <div>
                      <h2>Aprobacion de proveedores</h2>
                      <p>Solicitudes registradas en el portal externo</p>
                    </div>
                  </div>
                  <button
                    className="icon-action"
                    type="button"
                    onClick={handleLoadSuppliers}
                    aria-label="Actualizar proveedores"
                  >
                    <RefreshCw size={18} />
                  </button>
                </div>

                <div className="filter-row" role="group" aria-label="Filtro de proveedores">
                  {['PENDIENTE', 'OBSERVADO', 'APROBADO', 'RECHAZADO'].map((status) => (
                    <button
                      key={status}
                      type="button"
                      className={supplierStatus === status ? 'selected' : ''}
                      onClick={() => setSupplierStatus(status)}
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
                          RUC {supplier.ruc} · {supplier.email}
                        </span>
                      </div>
                      <span className="status-pill">{supplier.estadoHomologacion}</span>
                      <div className="decision-actions">
                        <button
                          type="button"
                          className="approve"
                          onClick={() => handleSupplierDecision(supplier.proveedorId, 'APROBAR')}
                        >
                          <CheckCircle2 size={16} />
                          Aprobar
                        </button>
                        <button
                          type="button"
                          className="observe"
                          onClick={() => handleSupplierDecision(supplier.proveedorId, 'OBSERVAR')}
                        >
                          <AlertTriangle size={16} />
                          Observar
                        </button>
                        <button
                          type="button"
                          className="reject"
                          onClick={() => handleSupplierDecision(supplier.proveedorId, 'RECHAZAR')}
                        >
                          <XCircle size={16} />
                          Rechazar
                        </button>
                      </div>
                    </article>
                  ))}
                </div>
              </section>
            ) : (
              <section className="approval-panel">
                <div className="panel-title">
                  <Building2 size={22} />
                  <div>
                    <h2>Portal proveedor</h2>
                    <p>Estado de registro y homologacion</p>
                  </div>
                </div>
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
              </section>
            )}
          </section>
        )}
      </section>
    </main>
  )
}

export default App
