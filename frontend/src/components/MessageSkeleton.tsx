import React from 'react'
import {
  Box,
  Skeleton,
  Paper
} from '@mui/material'
import { styled } from '@mui/material/styles'

const SkeletonContainer = styled(Box)(({ theme }) => ({
  marginBottom: theme.spacing(1),
  padding: theme.spacing(0, 1)
}))

const MessageSkeletonBubble = styled(Paper)(({ theme }) => ({
  padding: theme.spacing(1, 1.5),
  maxWidth: '70%',
  backgroundColor: theme.palette.background.paper,
  borderRadius: '18px 18px 18px 4px',
  boxShadow: theme.shadows[1]
}))

interface MessageSkeletonProps {
  isCurrentUser?: boolean
  count?: number
}

export const MessageSkeleton: React.FC<MessageSkeletonProps> = ({ 
  isCurrentUser = false, 
  count = 1 
}) => {
  return (
    <>
      {Array.from({ length: count }).map((_, index) => (
        <SkeletonContainer
          key={index}
          sx={{
            display: 'flex',
            justifyContent: isCurrentUser ? 'flex-end' : 'flex-start'
          }}
        >
          <MessageSkeletonBubble>
            {!isCurrentUser && (
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 0.5 }}>
                <Skeleton variant="circular" width={24} height={24} />
                <Skeleton variant="text" width={80} height={16} />
              </Box>
            )}
            <Skeleton 
              variant="text" 
              width={Math.random() * 200 + 100} 
              height={20} 
              sx={{ mb: 0.5 }} 
            />
            <Skeleton 
              variant="text" 
              width={Math.random() * 150 + 80} 
              height={20} 
            />
          </MessageSkeletonBubble>
        </SkeletonContainer>
      ))}
    </>
  )
}
