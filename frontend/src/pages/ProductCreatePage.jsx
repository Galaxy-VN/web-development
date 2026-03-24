import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { apiClient } from '../api/client'
import { extractErrorMessage } from '../utils/errors'
import ProductForm from '../components/ProductForm'

function ProductCreatePage() {
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState('')
  const navigate = useNavigate()

  const handleCreate = async (formData) => {
    setSubmitting(true)
    setError('')

    try {
      await apiClient.post('api/products', formData)
      navigate('/', { replace: true })
    } catch (err) {
      setError(extractErrorMessage(err))
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <section>
      {error && <p className="error">Lỗi: {error}</p>}
      <ProductForm mode="create" onSubmit={handleCreate} submitting={submitting} />
    </section>
  )
}

export default ProductCreatePage
