import { Alert, Center, Loader, Text } from '@mantine/core'

interface QueryStateProps {
  isLoading: boolean
  isError: boolean
  isEmpty?: boolean
  emptyMessage?: string
  errorMessage?: string
  children: React.ReactNode
}

/**
 * Loading, error and empty handling in one place so every page treats them the same
 */
export default function QueryState({
  isLoading,
  isError,
  isEmpty = false,
  emptyMessage = 'There is nothing here yet.',
  errorMessage = 'That could not be loaded. Please try again later.',
  children
}: QueryStateProps) {
  if (isLoading) {
    return <Center py="xl"><Loader /></Center>
  }

  if (isError) {
    return <Alert color="red" variant="light">{errorMessage}</Alert>
  }

  if (isEmpty) {
    return <Text c="dimmed" py="md">{emptyMessage}</Text>
  }

  return <>{children}</>
}
