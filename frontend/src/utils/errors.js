/**
 * Extracts validation error messages from axios error responses
 * @param {Error} err - Axios error object
 * @returns {string} - Human-readable error message
 */
export function extractErrorMessage(err) {
  const errorData = err?.response?.data

  // Check for validation problem (400 with errors object)
  if (errorData?.errors && typeof errorData.errors === 'object') {
    const errorList = Object.entries(errorData.errors)
      .flatMap(([field, messages]) => {
        if (Array.isArray(messages)) {
          return messages.map((msg) => `${field}: ${msg}`)
        }
        return [`${field}: ${messages}`]
      })

    if (errorList.length > 0) {
      return errorList.join('\n')
    }
  }

  // Fallback to title, message, or generic message
  return (
    errorData?.title ||
    errorData?.message ||
    err?.message ||
    'Đã xảy ra lỗi.'
  )
}
