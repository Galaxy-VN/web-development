import { backendOrigin } from '../api/client'

export const resolveImageUrl = (imageUrl) => {
  if (!imageUrl) {
    return 'https://placehold.co/600x400?text=No+Image'
  }

  if (/^https?:\/\//i.test(imageUrl)) {
    return imageUrl
  }

  const trimmed = imageUrl.startsWith('/') ? imageUrl.slice(1) : imageUrl
  return `${backendOrigin.replace(/\/$/, '')}/${trimmed}`
}
