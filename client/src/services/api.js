const apiBase = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5077'

export async function apiFetch(path, options = {}) {
  const response = await fetch(`${apiBase}${path}`, {
    headers: {
      'Content-Type': 'application/json',
      ...(options.headers ?? {}),
    },
    ...options,
  })

  if (!response.ok) {
    const message = await response.text()
    throw new Error(message || 'Request failed')
  }

  if (response.status === 204) return null
  return response.json()
}

export { apiBase }
