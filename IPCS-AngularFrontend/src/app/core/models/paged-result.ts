/**
 * Server-side paging envelope returned by list endpoints when called with
 * ?page= (e.g. Product?page=2&pageSize=20). Without paging params the same
 * endpoints return the old plain array.
 */
export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}
