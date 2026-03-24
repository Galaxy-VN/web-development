import { useEffect, useState } from 'react'
import { apiClient } from '../api/client'
import { resolveImageUrl } from '../utils/media'

const defaultForm = {
  name: '',
  price: '',
  description: '',
  categoryId: '',
  imageFile: null,
  removeImage: false
}

function ProductForm({ mode, initialData, onSubmit, submitting, onCancel }) {
  const [form, setForm] = useState(defaultForm)
  const [categories, setCategories] = useState([])
  const [loadingCategories, setLoadingCategories] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    if (initialData) {
      setForm({
        name: initialData.name ?? '',
        price: initialData.price ?? '',
        description: initialData.description ?? '',
        categoryId: initialData.categoryId ? String(initialData.categoryId) : '',
        imageFile: null,
        removeImage: false
      })
    } else {
      setForm(defaultForm)
    }
  }, [initialData])

  useEffect(() => {
    const fetchCategories = async () => {
      setLoadingCategories(true)
      try {
        const response = await apiClient.get('api/categories')
        setCategories(Array.isArray(response.data) ? response.data : [])
      } catch (err) {
        const message =
          err?.response?.data?.message || err?.message || 'Không thể tải danh mục.'
        setError(message)
        setCategories([])
      } finally {
        setLoadingCategories(false)
      }
    }

    fetchCategories()
  }, [])

  const validateForm = () => {
    if (!form.name.trim()) {
      return 'Tên sản phẩm là bắt buộc.'
    }

    const priceNum = Number(form.price)
    if (!Number.isFinite(priceNum) || priceNum <= 0) {
      return 'Giá sản phẩm phải lớn hơn 0.'
    }

    if (!form.categoryId) {
      return 'Vui lòng chọn danh mục.'
    }

    return ''
  }

  const handleSubmit = async (event) => {
    event.preventDefault()
    setError('')

    const validationError = validateForm()
    if (validationError) {
      setError(validationError)
      return
    }

    const formData = new FormData()
    formData.append('name', form.name.trim())
    formData.append('price', String(form.price))
    formData.append('description', form.description ?? '')
    formData.append('categoryId', String(form.categoryId))
    formData.append('removeImage', String(Boolean(form.removeImage)))

    if (form.imageFile) {
      formData.append('imageFile', form.imageFile)
    }

    await onSubmit(formData)
  }

  return (
    <section className="panel">
      <h2>{mode === 'create' ? 'Thêm sản phẩm' : `Sửa sản phẩm #${initialData?.id}`}</h2>

      {error && <p className="error">Lỗi: {error}</p>}

      <form className="form-grid" onSubmit={handleSubmit}>
        <label className="field">
          <span>Tên sản phẩm</span>
          <input
            type="text"
            value={form.name}
            onChange={(e) => setForm((prev) => ({ ...prev, name: e.target.value }))}
            placeholder="Nhập tên sản phẩm"
            required
          />
        </label>

        <label className="field">
          <span>Giá</span>
          <input
            type="number"
            min="0.01"
            step="0.01"
            value={form.price}
            onChange={(e) => setForm((prev) => ({ ...prev, price: e.target.value }))}
            placeholder="Ví dụ: 1500000"
            required
          />
        </label>

        <label className="field">
          <span>Danh mục</span>
          <select
            value={form.categoryId}
            onChange={(e) => setForm((prev) => ({ ...prev, categoryId: e.target.value }))}
            required
            disabled={loadingCategories}
          >
            <option value="">-- Chọn danh mục --</option>
            {categories.map((c) => (
              <option key={c.id} value={c.id}>
                {c.name}
              </option>
            ))}
          </select>
        </label>

        <label className="field field-description">
          <span>Mô tả</span>
          <textarea
            rows="3"
            value={form.description}
            onChange={(e) =>
              setForm((prev) => ({ ...prev, description: e.target.value }))
            }
            placeholder="Mô tả ngắn về sản phẩm"
          />
        </label>

        <label className="field">
          <span>{mode === 'create' ? 'Ảnh sản phẩm' : 'Thay ảnh mới (tuỳ chọn)'}</span>
          <input
            type="file"
            accept=".jpg,.jpeg,.png,.webp,image/*"
            onChange={(e) =>
              setForm((prev) => ({
                ...prev,
                imageFile: e.target.files?.[0] ?? null
              }))
            }
          />
        </label>

        {mode === 'edit' && initialData?.imageUrl && (
          <div className="preview-box">
            <span>Ảnh hiện tại</span>
            <img src={resolveImageUrl(initialData.imageUrl)} alt={initialData.name} />
          </div>
        )}

        {mode === 'edit' && (
          <label className="check-field">
            <input
              type="checkbox"
              checked={form.removeImage}
              onChange={(e) =>
                setForm((prev) => ({ ...prev, removeImage: e.target.checked }))
              }
            />
            <span>Xoá ảnh hiện tại nếu không upload ảnh mới</span>
          </label>
        )}

        <div className="actions">
          <button type="submit" className="btn primary" disabled={submitting}>
            {submitting
              ? 'Đang xử lý...'
              : mode === 'create'
                ? 'Thêm sản phẩm'
                : 'Lưu cập nhật'}
          </button>

          {mode === 'edit' && (
            <button type="button" className="btn" onClick={onCancel}>
              Huỷ
            </button>
          )}
        </div>
      </form>
    </section>
  )
}

export default ProductForm
