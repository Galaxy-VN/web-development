import { useEffect, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { apiClient } from '../api/client'
import { extractErrorMessage } from '../utils/errors'
import ProductForm from '../components/ProductForm'

function ProductEditPage() {
  const { id } = useParams()
  const navigate = useNavigate()

  const [product, setProduct] = useState(null)
  const [loading, setLoading] = useState(true)
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    const fetchProduct = async () => {
      setLoading(true)
      setError('')
      try {
        const response = await apiClient.get(`api/products/${id}`)
        setProduct(response.data)
      } catch (err) {
        setError(extractErrorMessage(err))
        setProduct(null)
      } finally {
        setLoading(false)
      }
    }

    fetchProduct()
  }, [id])

  const handleUpdate = async (formData) => {
    setSubmitting(true)
    setError('')

    try {
      await apiClient.put(`api/products/${id}`, formData)
      navigate(`/products/${id}`, { replace: true })
    } catch (err) {
      setError(extractErrorMessage(err))
    } finally {
      setSubmitting(false)
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
    <section>
      {error && <p className="error">Lỗi: {error}</p>}
      <ProductForm
        mode="edit"
        initialData={product}
        onSubmit={handleUpdate}
        submitting={submitting}
        onCancel={() => navigate(-1)}
      />
    </section>
  )
}

export default ProductEditPage
