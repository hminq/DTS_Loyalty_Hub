import { Fragment } from 'react'
import { Link } from 'react-router-dom'

import {
  Breadcrumb as BreadcrumbRoot,
  BreadcrumbItem,
  BreadcrumbLink,
  BreadcrumbList,
  BreadcrumbPage,
  BreadcrumbSeparator,
} from '../ui/breadcrumb'

function Breadcrumb({ items }) {
  return (
    <BreadcrumbRoot>
      <BreadcrumbList>
        {items.map((item, index) => {
          const isCurrent = index === items.length - 1

          return (
            <Fragment key={`${item.label}-${index}`}>
              {index > 0 ? <BreadcrumbSeparator /> : null}
              <BreadcrumbItem>
                {isCurrent || !item.to ? (
                  isCurrent
                    ? <BreadcrumbPage>{item.label}</BreadcrumbPage>
                    : <span className="truncate">{item.label}</span>
                ) : (
                  <BreadcrumbLink render={<Link to={item.to} />}>{item.label}</BreadcrumbLink>
                )}
              </BreadcrumbItem>
            </Fragment>
          )
        })}
      </BreadcrumbList>
    </BreadcrumbRoot>
  )
}

export { Breadcrumb }
