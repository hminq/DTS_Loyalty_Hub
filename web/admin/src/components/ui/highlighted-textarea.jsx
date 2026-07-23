import React, { useRef, forwardRef, useImperativeHandle } from 'react'
import { cn } from '../../lib/utils'

export const HighlightedTextarea = forwardRef(({ className, value, onChange, onSelect, onKeyUp, onClick, ...props }, ref) => {
  const textareaRef = useRef(null)
  const backdropRef = useRef(null)

  useImperativeHandle(ref, () => textareaRef.current)

  const handleScroll = (e) => {
    if (backdropRef.current) {
      backdropRef.current.scrollTop = e.target.scrollTop
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
    <div className="relative w-full">
      {/* Backdrop for highlighted text */}
      <div 
        ref={backdropRef}
        className={cn(
          "absolute inset-0 w-full h-full overflow-hidden whitespace-pre-wrap break-words border border-transparent px-3 py-2 text-sm",
          className
        )}
        style={{ color: 'transparent', caretColor: 'transparent', pointerEvents: 'none', fontFamily: 'inherit', letterSpacing: 'inherit', wordSpacing: 'inherit', lineHeight: 'inherit' }}
        aria-hidden="true"
      >
        <span className="text-foreground pointer-events-none">
          {renderHighlightedText(value)}
        </span>
        {/* Invisible extra space for trailing newlines to sync scroll correctly */}
        {value?.endsWith('\n') && <br />}
      </div>
      
      {/* Actual textarea */}
      <textarea
        ref={textareaRef}
        value={value}
        onChange={onChange}
        onSelect={onSelect}
        onScroll={handleScroll}
        onKeyUp={onKeyUp}
        onClick={onClick}
        className={cn(
          "relative z-10 flex w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50 resize-y",
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
HighlightedTextarea.displayName = 'HighlightedTextarea'
