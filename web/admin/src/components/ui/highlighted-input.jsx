import React, { useRef, forwardRef, useImperativeHandle } from 'react'
import { cn } from '../../lib/utils'

export const HighlightedInput = forwardRef(({ className, value, onChange, onSelect, onKeyUp, onClick, ...props }, ref) => {
  const inputRef = useRef(null)
  const backdropRef = useRef(null)

  useImperativeHandle(ref, () => inputRef.current)

  const handleScroll = (e) => {
    if (backdropRef.current) {
      backdropRef.current.scrollLeft = e.target.scrollLeft
    }
  }

  const renderHighlightedText = (text) => {
    if (!text) return null
    // Match variables like {{VariableName}}
    const regex = /(\{\{[^}]+\}\})/g
    const parts = text.split(regex)
    return parts.map((part, i) => {
      if (regex.test(part)) {
        return (
          <span key={i} className="text-primary bg-primary/15 rounded-sm">
            {part}
          </span>
        )
      }
      return <span key={i}>{part}</span>
    })
  }

  return (
    <div className="relative w-full h-10 flex items-center">
      {/* Backdrop for highlighted text */}
      <div 
        ref={backdropRef}
        className={cn(
          "absolute inset-0 w-full h-full overflow-hidden whitespace-pre px-3 py-2 border border-transparent text-sm",
          className
        )}
        style={{ color: 'transparent', caretColor: 'transparent', pointerEvents: 'none', fontFamily: 'inherit', letterSpacing: 'inherit', wordSpacing: 'inherit', lineHeight: 'inherit' }}
        aria-hidden="true"
      >
        <span className="text-foreground pointer-events-none">
          {renderHighlightedText(value)}
        </span>
      </div>
      
      {/* Actual input */}
      <input
        ref={inputRef}
        value={value}
        onChange={onChange}
        onSelect={onSelect}
        onScroll={handleScroll}
        onKeyUp={onKeyUp}
        onClick={onClick}
        className={cn(
          "relative z-10 flex h-10 w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm ring-offset-background file:border-0 file:bg-transparent file:text-sm file:font-medium placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50",
          "text-transparent caret-foreground",
          className
        )}
        style={{ color: 'transparent', caretColor: 'var(--color-foreground, #09090b)', fontFamily: 'inherit', letterSpacing: 'inherit', wordSpacing: 'inherit', lineHeight: 'inherit' }}
        spellCheck={false}
        {...props}
      />
    </div>
  )
})
HighlightedInput.displayName = 'HighlightedInput'
