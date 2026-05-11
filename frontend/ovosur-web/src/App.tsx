import { type FormEvent, useState } from 'react'
import {
  BadgeCheck,
  Building2,
  KeyRound,
  LayoutDashboard,
  LockKeyhole,
  PackageCheck,
  ShieldCheck,
  Truck,
} from 'lucide-react'
import { getApiError, login, type AuthSession } from './api/authClient'
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

            {session ? (
              <div className="session-box">
                <strong>{session.user.email}</strong>
                <span>{session.user.tipoUsuario}</span>
                <button type="button" onClick={handleLogout}>
                  Cerrar sesion
                </button>
              </div>
            ) : null}

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
      </section>
    </main>
  )
}

export default App
