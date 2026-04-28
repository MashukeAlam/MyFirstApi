import { useEffect, useMemo, useState } from 'react'
import { apiBase, apiFetch } from './services/api'

function App() {
  const [token, setToken] = useState('')
  const [form, setForm] = useState({ email: '', password: '' })
  const [authError, setAuthError] = useState('')
  const [items, setItems] = useState([])
  const [itemsError, setItemsError] = useState('')
  const [loadingItems, setLoadingItems] = useState(false)
  const [newItem, setNewItem] = useState({
    name: '',
    code: '',
    unit: '',
    itemType: 'raw',
    vatRate: 0,
    sdRate: 0,
  })
  const [createStatus, setCreateStatus] = useState('')

  const authHeaders = useMemo(() => {
    if (!token) return {}
    return { Authorization: `Bearer ${token}` }
  }, [token])

  useEffect(() => {
    setItemsError('')
    if (!token) {
      setItems([])
      return
    }

    let active = true
    setLoadingItems(true)
    apiFetch('/items', { headers: authHeaders })
      .then((data) => {
        if (!active) return
        setItems(data ?? [])
      })
      .catch((error) => {
        if (!active) return
        setItemsError(error.message)
      })
      .finally(() => {
        if (!active) return
        setLoadingItems(false)
      })

    return () => {
      active = false
    }
  }, [token, authHeaders])

  const handleLogin = async (event) => {
    event.preventDefault()
    setAuthError('')

    try {
      const result = await apiFetch('/auth/login', { 
        method: 'POST', 
        body: JSON.stringify({ 
          email: form.email, 
          password: form.password, 
        }), 
      }) 
      setToken(result.token) 
    } catch (error) {
      setAuthError(error.message)
    }
  }

  const handleCreateItem = async (event) => {
    event.preventDefault()
    setCreateStatus('')

    try {
      const created = await apiFetch('/items', {
        method: 'POST',
        headers: authHeaders,
        body: JSON.stringify({
          name: newItem.name,
          code: newItem.code,
          unit: newItem.unit,
          itemType: newItem.itemType,
          vatRate: Number(newItem.vatRate),
          sdRate: Number(newItem.sdRate),
        }),
      })

      setItems((prev) => [...prev, created])
      setNewItem({ name: '', code: '', unit: '', itemType: 'raw', vatRate: 0, sdRate: 0 })
      setCreateStatus('Item created')
    } catch (error) {
      setCreateStatus(error.message)
    }
  }

  return (
    <div className="min-h-screen bg-slate-950 text-slate-100">
      <header className="border-b border-slate-800 bg-slate-900/60">
        <div className="mx-auto flex max-w-6xl flex-wrap items-center justify-between gap-4 px-6 py-6">
          <div>
            <p className="text-sm uppercase tracking-[0.2em] text-slate-400">MyFirstApi</p>
            <h1 className="text-2xl font-semibold text-white">ERP Client</h1>
          </div>
          <div className="text-sm text-slate-300">
            API Base: <span className="font-mono text-slate-200">{apiBase}</span>
          </div>
        </div>
      </header>

      <main className="mx-auto grid max-w-6xl gap-8 px-6 py-10 lg:grid-cols-[360px_1fr]">
        <section className="rounded-2xl border border-slate-800 bg-slate-900/70 p-6 shadow-lg">
          <h2 className="text-lg font-semibold">Authenticate</h2>
          <p className="mt-1 text-sm text-slate-400">Sign in to access protected ERP endpoints.</p>

          <form className="mt-6 space-y-4" onSubmit={handleLogin}>
            <div>
              <label className="text-sm text-slate-300" htmlFor="email">
                Email
              </label>
              <input
                id="email"
                type="email"
                value={form.email}
                onChange={(event) => setForm((prev) => ({ ...prev, email: event.target.value }))}
                className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                placeholder="user@company.com"
                required
              />
            </div>
            <div>
              <label className="text-sm text-slate-300" htmlFor="password">
                Password
              </label>
              <input
                id="password"
                type="password"
                value={form.password}
                onChange={(event) => setForm((prev) => ({ ...prev, password: event.target.value }))}
                className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                placeholder="••••••••"
                required
              />
            </div>
            <button
              type="submit"
              className="w-full rounded-lg bg-indigo-500 px-4 py-2 text-sm font-semibold text-white transition hover:bg-indigo-400"
            >
              Sign in
            </button>
          </form>

          {authError && (
            <p className="mt-3 text-sm text-red-400">{authError}</p>
          )}

          <div className="mt-6 rounded-lg border border-dashed border-slate-700 p-4 text-xs text-slate-400">
            <p className="font-semibold text-slate-300">JWT Token</p>
            <p className="mt-2 break-all">{token || 'Sign in to get a token.'}</p>
          </div>
        </section>

        <section className="space-y-6">
          <div className="rounded-2xl border border-slate-800 bg-slate-900/70 p-6 shadow-lg">
            <div className="flex items-center justify-between">
              <div>
                <h2 className="text-lg font-semibold">Items</h2>
                <p className="text-sm text-slate-400">Read and create inventory items.</p>
              </div>
              {loadingItems && <span className="text-xs text-slate-400">Loading…</span>}
            </div>

            {itemsError && (
              <p className="mt-4 text-sm text-red-400">{itemsError}</p>
            )}

            {!itemsError && (
              <div className="mt-4 overflow-hidden rounded-xl border border-slate-800">
                <table className="w-full text-left text-sm">
                  <thead className="bg-slate-900">
                    <tr className="text-slate-300">
                      <th className="px-4 py-3 font-medium">Name</th>
                      <th className="px-4 py-3 font-medium">Code</th>
                      <th className="px-4 py-3 font-medium">Unit</th>
                      <th className="px-4 py-3 font-medium">VAT</th>
                      <th className="px-4 py-3 font-medium">SD</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-800">
                    {items.length === 0 && (
                      <tr>
                        <td className="px-4 py-4 text-slate-400" colSpan={5}>
                          {token ? 'No items yet.' : 'Sign in to load items.'}
                        </td>
                      </tr>
                    )}
                    {items.map((item) => (
                      <tr key={item.id} className="text-slate-200">
                        <td className="px-4 py-3">{item.name}</td>
                        <td className="px-4 py-3">{item.code}</td>
                        <td className="px-4 py-3">{item.unit}</td>
                        <td className="px-4 py-3">{item.vatRate}%</td>
                        <td className="px-4 py-3">{item.sdRate}%</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>

          <div className="rounded-2xl border border-slate-800 bg-slate-900/70 p-6 shadow-lg">
            <h2 className="text-lg font-semibold">Create Item</h2>
            <p className="text-sm text-slate-400">POST /items</p>

            <form className="mt-4 grid gap-4 md:grid-cols-2" onSubmit={handleCreateItem}>
              <div>
                <label className="text-sm text-slate-300" htmlFor="name">
                  Name
                </label>
                <input
                  id="name"
                  value={newItem.name}
                  onChange={(event) => setNewItem((prev) => ({ ...prev, name: event.target.value }))}
                  className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                  required
                />
              </div>
              <div>
                <label className="text-sm text-slate-300" htmlFor="code">
                  Code
                </label>
                <input
                  id="code"
                  value={newItem.code}
                  onChange={(event) => setNewItem((prev) => ({ ...prev, code: event.target.value }))}
                  className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                  required
                />
              </div>
              <div>
                <label className="text-sm text-slate-300" htmlFor="unit">
                  Unit
                </label>
                <input
                  id="unit"
                  value={newItem.unit}
                  onChange={(event) => setNewItem((prev) => ({ ...prev, unit: event.target.value }))}
                  className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                  required
                />
              </div>
              <div>
                <label className="text-sm text-slate-300" htmlFor="itemType">
                  Item Type
                </label>
                <select
                  id="itemType"
                  value={newItem.itemType}
                  onChange={(event) => setNewItem((prev) => ({ ...prev, itemType: event.target.value }))}
                  className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                >
                  <option value="raw">Raw</option>
                  <option value="finished">Finished</option>
                  <option value="both">Both</option>
                </select>
              </div>
              <div>
                <label className="text-sm text-slate-300" htmlFor="vatRate">
                  VAT Rate (%)
                </label>
                <input
                  id="vatRate"
                  type="number"
                  step="0.01"
                  value={newItem.vatRate}
                  onChange={(event) => setNewItem((prev) => ({ ...prev, vatRate: event.target.value }))}
                  className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                />
              </div>
              <div>
                <label className="text-sm text-slate-300" htmlFor="sdRate">
                  SD Rate (%)
                </label>
                <input
                  id="sdRate"
                  type="number"
                  step="0.01"
                  value={newItem.sdRate}
                  onChange={(event) => setNewItem((prev) => ({ ...prev, sdRate: event.target.value }))}
                  className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                />
              </div>
              <div className="md:col-span-2">
                <button
                  type="submit"
                  className="w-full rounded-lg bg-emerald-500 px-4 py-2 text-sm font-semibold text-slate-950 transition hover:bg-emerald-400"
                >
                  Create Item
                </button>
              </div>
            </form>

            {createStatus && (
              <p className="mt-3 text-sm text-slate-300">{createStatus}</p>
            )}
          </div>
        </section>
      </main>
    </div>
  )
}

export default App
