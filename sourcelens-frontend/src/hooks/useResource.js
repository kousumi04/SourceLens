import { useCallback, useEffect, useState } from "react";
import { api } from "../api/client";

/**
 * Loads a resource from the live SourceLens API. If the API can't be
 * reached (backend not running / CORS / wrong URL), it falls back to
 * local demo data so the dashboard stays fully interactive — creates,
 * edits, and deletes are then simulated in local state.
 */
export function useResource(endpoint, mockData) {
  const [data, setData] = useState([]);
  const [loading, setLoading] = useState(true);
  const [isDemo, setIsDemo] = useState(false);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const res = await api.get(`/${endpoint}`);
      setData(Array.isArray(res.data) ? res.data : []);
      setIsDemo(false);
    } catch (err) {
      setData(mockData);
      setIsDemo(true);
    } finally {
      setLoading(false);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [endpoint]);

  useEffect(() => {
    load();
  }, [load]);

  const create = useCallback(
    async (item) => {
      if (isDemo) {
        const newItem = { ...item, id: Date.now() };
        setData((prev) => [newItem, ...prev]);
        return newItem;
      }
      const res = await api.post(`/${endpoint}`, item);
      setData((prev) => [res.data, ...prev]);
      return res.data;
    },
    [endpoint, isDemo]
  );

  const update = useCallback(
    async (id, item) => {
      if (isDemo) {
        setData((prev) => prev.map((d) => (d.id === id ? { ...d, ...item } : d)));
        return;
      }
      await api.put(`/${endpoint}/${id}`, item);
      setData((prev) => prev.map((d) => (d.id === id ? { ...d, ...item } : d)));
    },
    [endpoint, isDemo]
  );

  const remove = useCallback(
    async (id) => {
      if (isDemo) {
        setData((prev) => prev.filter((d) => d.id !== id));
        return;
      }
      await api.delete(`/${endpoint}/${id}`);
      setData((prev) => prev.filter((d) => d.id !== id));
    },
    [endpoint, isDemo]
  );

  return { data, loading, isDemo, reload: load, create, update, remove };
}
