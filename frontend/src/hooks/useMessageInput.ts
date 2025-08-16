import { useState, useCallback, KeyboardEvent } from 'react'

interface UseMessageInputReturn {
  value: string
  isSubmitting: boolean
  setValue: (value: string) => void
  clearValue: () => void
  setSubmitting: (isSubmitting: boolean) => void
  handleSubmit: (onSubmit: (message: string) => Promise<void> | void) => Promise<void>
  handleKeyPress: (onSubmit: (message: string) => Promise<void> | void) => (event: KeyboardEvent) => Promise<void>
}

export const useMessageInput = (): UseMessageInputReturn => {
  const [value, setValue] = useState<string>('')
  const [isSubmitting, setIsSubmitting] = useState(false)

  const clearValue = useCallback(() => {
    setValue('')
  }, [])

  const setSubmitting = useCallback((submitting: boolean) => {
    setIsSubmitting(submitting)
  }, [])

  const handleSubmit = useCallback(async (onSubmit: (message: string) => Promise<void> | void): Promise<void> => {
    const trimmedValue = value.trim()
    
    if (!trimmedValue || isSubmitting) {
      return
    }

    try {
      setIsSubmitting(true)
      await onSubmit(trimmedValue)
      setValue('')
    } catch (error) {
      console.error('Error submitting message:', error)
      throw error
    } finally {
      setIsSubmitting(false)
    }
  }, [value, isSubmitting])

  const handleKeyPress = useCallback((onSubmit: (message: string) => Promise<void> | void) => {
    return async (event: KeyboardEvent): Promise<void> => {
      if (event.key === 'Enter' && !event.shiftKey) {
        event.preventDefault()
        await handleSubmit(onSubmit)
      }
    }
  }, [handleSubmit])

  return {
    value,
    isSubmitting,
    setValue,
    clearValue,
    setSubmitting,
    handleSubmit,
    handleKeyPress
  }
}
