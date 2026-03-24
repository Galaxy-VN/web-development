import { useEffect, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { apiClient } from '../api/client'
import { resolveImageUrl } from '../utils/media'

function ProductDetailPage() {
  const { id } = useParams()
  const navigate = useNavigate()

  const [product, setProduct] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [deleting, setDeleting] = useState(false)

  useEffect(() => {
    const fetchProduct = async () => {
      setLoading(true)
      setError('')
      try {
        const response = await apiClient.get(`api/products/${id}`)
        setProduct(response.data)
      } catch (err) {
        const messageText =
          err?.response?.data?.message || err?.message || 'Không thể tải chi tiết.'
        setError(messageText)
        setProduct(null)
      } finally {
        setLoading(false)
      }
    }

    fetchProduct()
  }, [id])

  const handleDelete = async () => {
    const confirmed = window.confirm('Bạn có chắc muốn xoá sản phẩm này?')
    if (!confirmed) {
      return
    }

    setDeleting(true)
    setError('')
    try {
      await apiClient.delete(`api/products/${id}`)
      navigate('/', { replace: true })
    } catch (err) {
      const messageText =
        err?.response?.data?.message || err?.message || 'Không thể xoá sản phẩm.'
      setError(messageText)
    } finally {
      setDeleting(false)
    }
  }

  if (loading) {
    return <div className="empty">Đang tải dữ liệu...</div>
  }

  if (!product) {
    return (
      <div className="empty">
        Không tìm thấy sản phẩm. <Link to="/">Quay lại danh sách</Link>
      </div>
    )
  }

  return (
    <section className="panel">
      <h2>Chi tiết sản phẩm</h2>
      {error && <p className="error">Lỗi: {error}</p>}

      <div className="detail-grid">
        <img
          className="detail-image"
          src={resolveImageUrl(product.imageUrl)}
          alt={product.name || 'Hình sản phẩm'}
        />

        <div className="detail-content">
          <h3>{product.name}</h3>
          <p className="price">{(product.price || 0).toLocaleString('vi-VN')} VND</p>
          <p className="category">Danh mục: {product.categoryName || 'Chưa cập nhật'}</p>
          <p className="desc">{product.description || 'Không có mô tả.'}</p>

          <div className="actions">
            <Link className="btn" to="/">
              Quay lại
            </Link>
            <Link className="btn" to={`/products/${product.id}/edit`}>
              Sửa
            </Link>
            <button
              type="button"
              className="btn danger"
              onClick={handleDelete}
              disabled={deleting}
            >
              {deleting ? 'Đang xoá...' : 'Xoá'}
            </button>
          </div>
        </div>
      </div>
    </section>
  )
}

export default ProductDetailPage
