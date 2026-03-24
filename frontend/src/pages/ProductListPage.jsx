import { useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { apiClient } from '../api/client'
import { resolveImageUrl } from '../utils/media'

const PAGE_SIZE = 8

function ProductListPage() {
  const [products, setProducts] = useState([])
  const [loading, setLoading] = useState(true)
  const [deletingId, setDeletingId] = useState(null)
  const [error, setError] = useState('')
  const [message, setMessage] = useState('')
  const [currentPage, setCurrentPage] = useState(1)

  const fetchProducts = async () => {
    setLoading(true)
    setError('')
    try {
      const response = await apiClient.get('api/products')
      const rows = Array.isArray(response.data) ? response.data : []
      setProducts(rows)
    } catch (err) {
      const messageText =
        err?.response?.data?.message || err?.message || 'Không thể tải sản phẩm.'
      setError(messageText)
      setProducts([])
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    fetchProducts()
  }, [])

  const totalPages = Math.max(1, Math.ceil(products.length / PAGE_SIZE))

  useEffect(() => {
    if (currentPage > totalPages) {
      setCurrentPage(totalPages)
    }
  }, [currentPage, totalPages])

  const pagedProducts = useMemo(() => {
    const start = (currentPage - 1) * PAGE_SIZE
    return products.slice(start, start + PAGE_SIZE)
  }, [products, currentPage])

  const handleDelete = async (id) => {
    const confirmed = window.confirm('Bạn có chắc muốn xoá sản phẩm này?')
    if (!confirmed) {
      return
    }

    setDeletingId(id)
    setError('')
    setMessage('')
    try {
      await apiClient.delete(`api/products/${id}`)
      setMessage('Xoá sản phẩm thành công.')
      await fetchProducts()
    } catch (err) {
      const messageText =
        err?.response?.data?.message || err?.message || 'Không thể xoá sản phẩm.'
      setError(messageText)
    } finally {
      setDeletingId(null)
    }
  }

  return (
    <section>
      <div className="toolbar">
        <h2>Danh sách sản phẩm</h2>
        <button className="btn" onClick={fetchProducts} disabled={loading}>
          {loading ? 'Đang tải...' : 'Tải lại'}
        </button>
      </div>

      {error && <p className="error">Lỗi: {error}</p>}
      {message && <p className="success">{message}</p>}

      {!loading && !error && products.length === 0 && (
        <div className="empty">Không có sản phẩm nào.</div>
      )}

      <section className="grid">
        {pagedProducts.map((product) => (
          <article className="card" key={product.id}>
            <img
              className="thumb"
              src={resolveImageUrl(product.imageUrl)}
              alt={product.name || 'Hình sản phẩm'}
            />

            <div className="body">
              <h3>{product.name || 'Không có tên'}</h3>
              <p className="price">
                {(product.price || 0).toLocaleString('vi-VN')} VND
              </p>
              <p className="category">
                Danh mục: {product.categoryName || 'Chưa cập nhật'}
              </p>
              <p className="desc">{product.description || 'Không có mô tả.'}</p>

              <div className="card-actions">
                <Link className="btn small" to={`/products/${product.id}`}>
                  Xem
                </Link>
                <Link className="btn small" to={`/products/${product.id}/edit`}>
                  Sửa
                </Link>
                <button
                  type="button"
                  className="btn small danger"
                  onClick={() => handleDelete(product.id)}
                  disabled={deletingId === product.id}
                >
                  {deletingId === product.id ? 'Đang xoá...' : 'Xoá'}
                </button>
              </div>
            </div>
          </article>
        ))}
      </section>

      {products.length > PAGE_SIZE && (
        <div className="pagination">
          <button
            className="btn"
            disabled={currentPage <= 1}
            onClick={() => setCurrentPage((p) => p - 1)}
          >
            Trang trước
          </button>
          <span>
            Trang {currentPage}/{totalPages}
          </span>
          <button
            className="btn"
            disabled={currentPage >= totalPages}
            onClick={() => setCurrentPage((p) => p + 1)}
          >
            Trang sau
          </button>
        </div>
      )}
    </section>
  )
}

export default ProductListPage
