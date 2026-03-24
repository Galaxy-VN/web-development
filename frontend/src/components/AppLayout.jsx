import { Link, NavLink, Outlet } from 'react-router-dom'

function AppLayout() {
  return (
    <main className="page">
      <div className="ambient a" aria-hidden="true"></div>
      <div className="ambient b" aria-hidden="true"></div>

      <header className="header">
        <div>
          <p className="eyebrow">WebBanHang</p>
          <h1>Quản lý sản phẩm</h1>
        </div>
      </header>

      <nav className="top-nav">
        <NavLink to="/" end className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`}>
          Danh sách
        </NavLink>
        <NavLink to="/products/new" className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`}>
          Thêm sản phẩm
        </NavLink>
        <Link className="nav-link muted" to="http://localhost:5272/swagger" target="_blank" rel="noreferrer">
          Swagger API
        </Link>
      </nav>

      <Outlet />
    </main>
  )
}

export default AppLayout
