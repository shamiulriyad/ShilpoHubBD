export default function Table({ columns = [], rows = [], emptyLabel = 'No records match this view.' }) {
  return (
    <div className="overflow-x-auto rounded-xl border border-border bg-surface">
      <table className="w-full min-w-[560px] text-left text-sm">
        <thead>
          <tr className="border-b border-border bg-background/60 text-xs uppercase tracking-wide text-body/60">
            {columns.map((column) => (
              <th scope="col" key={column} className="px-4 py-3 font-semibold">
                {column}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {rows.length === 0 && <tr><td colSpan={Math.max(1, columns.length)} className="px-4 py-10 text-center text-muted">{emptyLabel}</td></tr>}
          {rows.map((row, index) => (
            <tr key={index} className="border-b border-border last:border-0 hover:bg-background/40">
              {columns.map((column) => (
                <td key={column} className="px-4 py-3 text-body">
                  {row[column]}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
