﻿#undef SERVERSIDE
using System;

namespace Core.Net.Kcp
{
#if SERVERSIDE
    [System.Diagnostics.Tracing.EventSource(Name = "System.Buffers.ArrayPoolEventSource")]
    internal sealed class ArrayPoolEventSource : System.Diagnostics.Tracing.EventSource {
        internal readonly static ArrayPoolEventSource Log = new ArrayPoolEventSource();

        /// <summary>The reason for a BufferAllocated event.</summary>
        internal enum BufferAllocatedReason : int {
            /// <summary>The pool is allocating a buffer to be pooled in a bucket.</summary>
            Pooled,
            /// <summary>The requested buffer size was too large to be pooled.</summary>
            OverMaximumSize,
            /// <summary>The pool has already allocated for pooling as many buffers of a particular size as it's allowed.</summary>
            PoolExhausted
        }

        /// <summary>
        /// Event for when a buffer is rented.  This is invoked once for every successful call to Rent,
        /// regardless of whether a buffer is allocated or a buffer is taken from the pool.  In a
        /// perfect situation where all rented buffers are returned, we expect to see the number
        /// of BufferRented events exactly match the number of BuferReturned events, with the number
        /// of BufferAllocated events being less than or equal to those numbers (ideally significantly
        /// less than).
        /// </summary>
        [System.Diagnostics.Tracing.Event(1, Level = System.Diagnostics.Tracing.EventLevel.Verbose)]
        internal unsafe void BufferRented(int bufferId, int bufferSize, int poolId, int bucketId) {
            EventData* payload = stackalloc EventData[4];
            payload[0].Size = sizeof(int);
            payload[0].DataPointer = ((IntPtr)(&bufferId));
            payload[1].Size = sizeof(int);
            payload[1].DataPointer = ((IntPtr)(&bufferSize));
            payload[2].Size = sizeof(int);
            payload[2].DataPointer = ((IntPtr)(&poolId));
            payload[3].Size = sizeof(int);
            payload[3].DataPointer = ((IntPtr)(&bucketId));
            WriteEventCore(1, 4, payload);
        }

        /// <summary>
        /// Event for when a buffer is allocated by the pool.  In an ideal situation, the number
        /// of BufferAllocated events is significantly smaller than the number of BufferRented and
        /// BufferReturned events.
        /// </summary>
        [System.Diagnostics.Tracing.Event(2, Level = System.Diagnostics.Tracing.EventLevel.Informational)]
        internal unsafe void BufferAllocated(int bufferId, int bufferSize, int poolId, int bucketId, BufferAllocatedReason reason) {
            EventData* payload = stackalloc EventData[5];
            payload[0].Size = sizeof(int);
            payload[0].DataPointer = ((IntPtr)(&bufferId));
            payload[1].Size = sizeof(int);
            payload[1].DataPointer = ((IntPtr)(&bufferSize));
            payload[2].Size = sizeof(int);
            payload[2].DataPointer = ((IntPtr)(&poolId));
            payload[3].Size = sizeof(int);
            payload[3].DataPointer = ((IntPtr)(&bucketId));
            payload[4].Size = sizeof(BufferAllocatedReason);
            payload[4].DataPointer = ((IntPtr)(&reason));
            WriteEventCore(2, 5, payload);
        }

        /// <summary>
        /// Event raised when a buffer is returned to the pool.  This event is raised regardless of whether
        /// the returned buffer is stored or dropped.  In an ideal situation, the number of BufferReturned
        /// events exactly matches the number of BufferRented events.
        /// </summary>
        [System.Diagnostics.Tracing.Event(3, Level = System.Diagnostics.Tracing.EventLevel.Verbose)]
        internal void BufferReturned(int bufferId, int bufferSize, int poolId) => WriteEvent(3, bufferId, bufferSize, poolId);
    }
#endif

	/// <summary>
	/// Provides a resource pool that enables reusing instances of type <see cref="T:T[]"/>. 
	/// </summary>
	/// <remarks>
	/// <para>
	/// Renting and returning buffers with an <see cref="ArrayPool{T}"/> can increase performance
	/// in situations where arrays are created and destroyed frequently, resulting in significant
	/// memory pressure on the garbage collector.
	/// </para>
	/// <para>
	/// This class is thread-safe.  All members may be used by multiple threads concurrently.
	/// </para>
	/// </remarks>
	public abstract class ArrayPool<T>
	{
#if SERVERSIDE
        /// <summary>The lazily-initialized shared pool instance.</summary>
        private static ArrayPool<T> s_sharedInstance = null;

        /// <summary>
        /// Retrieves a shared <see cref="ArrayPool{T}"/> instance.
        /// </summary>
        /// <remarks>
        /// The shared pool provides a default implementation of <see cref="ArrayPool{T}"/>
        /// that's intended for general applicability.  It maintains arrays of multiple sizes, and 
        /// may hand back a larger array than was actually requested, but will never hand back a smaller 
        /// array than was requested. Renting a buffer from it with <see cref="Rent"/> will result in an 
        /// existing buffer being taken from the pool if an appropriate buffer is available or in a new 
        /// buffer being allocated if one is not available.
        /// </remarks>
        public static ArrayPool<T> Shared {
            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get {
                return System.Threading.Volatile.Read(ref s_sharedInstance) ?? EnsureSharedCreated();
            }
        }

        /// <summary>Ensures that <see cref="s_sharedInstance"/> has been initialized to a pool and returns it.</summary>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private static ArrayPool<T> EnsureSharedCreated() {
            System.Threading.Interlocked.CompareExchange(ref s_sharedInstance, Create(), null);
            return s_sharedInstance;
        }
#endif
		/// <summary>
		/// Creates a new <see cref="ArrayPool{T}"/> instance using default configuration options.
		/// </summary>
		/// <returns>A new <see cref="ArrayPool{T}"/> instance.</returns>
		public static ArrayPool<T> Create()
		{
			return new DefaultArrayPool<T>();
		}

		/// <summary>
		/// Creates a new <see cref="ArrayPool{T}"/> instance using custom configuration options.
		/// </summary>
		/// <param name="maxArrayLength">The maximum length of array instances that may be stored in the pool.</param>
		/// <param name="maxArraysPerBucket">
		/// The maximum number of array instances that may be stored in each bucket in the pool.  The pool
		/// groups arrays of similar lengths into buckets for faster access.
		/// </param>
		/// <returns>A new <see cref="ArrayPool{T}"/> instance with the specified configuration options.</returns>
		/// <remarks>
		/// The created pool will group arrays into buckets, with no more than <paramref name="maxArraysPerBucket"/>
		/// in each bucket and with those arrays not exceeding <paramref name="maxArrayLength"/> in length.
		/// </remarks>
		public static ArrayPool<T> Create( int maxArrayLength, int maxArraysPerBucket )
		{
			return new DefaultArrayPool<T>( maxArrayLength, maxArraysPerBucket );
		}
		/// <summary>
		/// .net系统自己管理
		/// </summary>
		/// <returns></returns>
		public static ArrayPool<T> ToSystem()
		{
			return new SysArrayPool<T>();
		}
		/// <summary>
		/// Retrieves a buffer that is at least the requested length.
		/// </summary>
		/// <param name="minimumLength">The minimum length of the array needed.</param>
		/// <returns>
		/// An <see cref="T:T[]"/> that is at least <paramref name="minimumLength"/> in length.
		/// </returns>
		/// <remarks>
		/// This buffer is loaned to the caller and should be returned to the same pool via 
		/// <see cref="Return"/> so that it may be reused in subsequent usage of <see cref="Rent"/>.  
		/// It is not a fatal error to not return a rented buffer, but failure to do so may lead to 
		/// decreased application performance, as the pool may need to create a new buffer to replace
		/// the one lost.
		/// </remarks>
		public abstract T[] Rent( int minimumLength );

		/// <summary>
		/// Returns to the pool an array that was previously obtained via <see cref="Rent"/> on the same 
		/// <see cref="ArrayPool{T}"/> instance.
		/// </summary>
		/// <param name="array">
		/// The buffer previously obtained from <see cref="Rent"/> to return to the pool.
		/// </param>
		/// <param name="clearArray">
		/// If <c>true</c> and if the pool will store the buffer to enable subsequent reuse, <see cref="Return"/>
		/// will clear <paramref name="array"/> of its contents so that a subsequent consumer via <see cref="Rent"/> 
		/// will not see the previous consumer's content.  If <c>false</c> or if the pool will release the buffer,
		/// the array's contents are left unchanged.
		/// </param>
		/// <remarks>
		/// Once a buffer has been returned to the pool, the caller gives up all ownership of the buffer 
		/// and must not use it. The reference returned from a given call to <see cref="Rent"/> must only be
		/// returned via <see cref="Return"/> once.  The default <see cref="ArrayPool{T}"/>
		/// may hold onto the returned buffer in order to rent it again, or it may release the returned buffer
		/// if it's determined that the pool already has enough buffers stored.
		/// </remarks>
		public abstract void Return( T[] array, bool clearArray = false );
	}

	internal sealed class SysArrayPool<T> : ArrayPool<T>
	{
		public override T[] Rent( int minimumLength )
		{
			return new T[minimumLength];
		}

		public override void Return( T[] array, bool clearArray = false )
		{

		}
	}

	internal sealed partial class DefaultArrayPool<T> : ArrayPool<T>
	{
		/// <summary>The default maximum length of each array in the pool (2^20).</summary>
		private const int DEFAULT_MAX_ARRAY_LENGTH = 1024 * 1024;
		/// <summary>The default maximum number of arrays per bucket that are available for rent.</summary>
		private const int DEFAULT_MAX_NUMBER_OF_ARRAYS_PER_BUCKET = 50;
		/// <summary>Lazily-allocated empty array used when arrays of length 0 are requested.</summary>
		private static T[] _sEmptyArray; // we support contracts earlier than those with Array.Empty<T>()

		private readonly Bucket[] _buckets;

		internal DefaultArrayPool() : this( DEFAULT_MAX_ARRAY_LENGTH, DEFAULT_MAX_NUMBER_OF_ARRAYS_PER_BUCKET )
		{
		}

		internal DefaultArrayPool( int maxArrayLength, int maxArraysPerBucket )
		{
			if ( maxArrayLength <= 0 )
				throw new ArgumentOutOfRangeException( nameof( maxArrayLength ) );

			if ( maxArraysPerBucket <= 0 )
				throw new ArgumentOutOfRangeException( nameof( maxArraysPerBucket ) );

			// Our bucketing algorithm has a min length of 2^4 and a max length of 2^30.
			// Constrain the actual max used to those values.
			const int minimumArrayLength = 0x10, maximumArrayLength = 0x40000000;
			if ( maxArrayLength > maximumArrayLength )
				maxArrayLength = maximumArrayLength;
			else if ( maxArrayLength < minimumArrayLength )
				maxArrayLength = minimumArrayLength;

			// Create the buckets.
			int poolId = this.id;
			int maxBuckets = Utilities.SelectBucketIndex( maxArrayLength );
			var buckets = new Bucket[maxBuckets + 1];
			for ( int i = 0; i < buckets.Length; i++ )
				buckets[i] = new Bucket( Utilities.GetMaxSizeForBucket( i ), maxArraysPerBucket, poolId );
			this._buckets = buckets;
		}

		/// <summary>Gets an ID for the pool to use with events.</summary>
		private int id => this.GetHashCode();

		public override T[] Rent( int minimumLength )
		{
			// Arrays can't be smaller than zero.  We allow requesting zero-length arrays (even though
			// pooling such an array isn't valuable) as it's a valid length array, and we want the pool
			// to be usable in general instead of using `new`, even for computed lengths.
			if ( minimumLength < 0 )
			{
				throw new ArgumentOutOfRangeException( nameof(minimumLength) );
			}
			if ( minimumLength == 0 )
			{
				// No need for events with the empty array.  Our pool is effectively infinite
				// and we'll never allocate for rents and never store for returns.
				return _sEmptyArray ?? ( _sEmptyArray = new T[0] );
			}
#if SERVERSIDE
            var log = ArrayPoolEventSource.Log;
#endif
			T[] buffer;

			int index = Utilities.SelectBucketIndex( minimumLength );
			if ( index < this._buckets.Length )
			{
				// Search for an array starting at the 'index' bucket. If the bucket is empty, bump up to the
				// next higher bucket and try that one, but only try at most a few buckets.
				const int maxBucketsToTry = 2;
				int i = index;
				do
				{
					// Attempt to rent from the bucket.  If we get a buffer from it, return it.
					buffer = this._buckets[i].Rent();
					if ( buffer != null )
					{
#if SERVERSIDE
                        if(log.IsEnabled()) {
                            log.BufferRented(buffer.GetHashCode(), buffer.Length, Id, _buckets[i].Id);
                        }
#endif
						return buffer;
					}
				}
				while ( ++i < this._buckets.Length && i != index + maxBucketsToTry );

				// The pool was exhausted for this buffer size.  Allocate a new buffer with a size corresponding
				// to the appropriate bucket.
				buffer = new T[this._buckets[index].bufferLength];
			}
			else
			{
				// The request was for a size too large for the pool.  Allocate an array of exactly the requested length.
				// When it's returned to the pool, we'll simply throw it away.
				buffer = new T[minimumLength];
			}
#if SERVERSIDE
            if(log.IsEnabled()) {
                int bufferId = buffer.GetHashCode(), bucketId = -1; // no bucket for an on-demand allocated buffer
                log.BufferRented(bufferId, buffer.Length, Id, bucketId);
                log.BufferAllocated(bufferId, buffer.Length, Id, bucketId, index >= _buckets.Length ?
                    ArrayPoolEventSource.BufferAllocatedReason.OverMaximumSize : ArrayPoolEventSource.BufferAllocatedReason.PoolExhausted);
            }
#endif

			return buffer;
		}

		public override void Return( T[] array, bool clearArray )
		{
			if ( array == null )
			{
				throw new ArgumentNullException( nameof(array) );
			}
			if ( array.Length == 0 )
			{
				// Ignore empty arrays.  When a zero-length array is rented, we return a singleton
				// rather than actually taking a buffer out of the lowest bucket.
				return;
			}

			// Determine with what bucket this array length is associated
			int bucket = Utilities.SelectBucketIndex( array.Length );

			// If we can tell that the buffer was allocated, drop it. Otherwise, check if we have space in the pool
			if ( bucket < this._buckets.Length )
			{
				// Clear the array if the user requests
				if ( clearArray )
				{
					Array.Clear( array, 0, array.Length );
				}

				// Return the buffer to its bucket.  In the future, we might consider having Return return false
				// instead of dropping a bucket, in which case we could try to return to a lower-sized bucket,
				// just as how in Rent we allow renting from a higher-sized bucket.
				this._buckets[bucket].Return( array );
			}
#if SERVERSIDE
            // Log that the buffer was returned
            var log = ArrayPoolEventSource.Log;
            if(log.IsEnabled()) {
                log.BufferReturned(array.GetHashCode(), array.Length, Id);
            }
#endif
		}
	}
	internal sealed partial class DefaultArrayPool<T> : ArrayPool<T>
	{
		/// <summary>Provides a thread-safe bucket containing buffers that can be Rent'd and Return'd.</summary>
		private sealed class Bucket
		{
			internal readonly int bufferLength;
			private readonly T[][] _buffers;
			private readonly int _poolId;
#if SERVERSIDE
            private System.Threading.SpinLock _lock; // do not make this readonly; it's a mutable struct
#endif
			private int _index;

			/// <summary>
			/// Creates the pool with numberOfBuffers arrays where each buffer is of bufferLength length.
			/// </summary>
			internal Bucket( int bufferLength, int numberOfBuffers, int poolId )
			{
#if SERVERSIDE
                _lock = new System.Threading.SpinLock(System.Diagnostics.Debugger.IsAttached); // only enable thread tracking if debugger is attached; it adds non-trivial overheads to Enter/Exit
#endif
				this._buffers = new T[numberOfBuffers][];
				this.bufferLength = bufferLength;
				this._poolId = poolId;
			}

			/// <summary>Gets an ID for the bucket to use with events.</summary>
			internal int id => this.GetHashCode();

			/// <summary>Takes an array from the bucket.  If the bucket is empty, returns null.</summary>
			internal T[] Rent()
			{
				T[][] buffers = this._buffers;
				T[] buffer = null;

				// While holding the lock, grab whatever is at the next available index and
				// update the index.  We do as little work as possible while holding the spin
				// lock to minimize contention with other threads.  The try/finally is
				// necessary to properly handle thread aborts on platforms which have them.
#if SERVERSIDE
				bool lockTaken = false;
#endif
				bool allocateBuffer = false;
				try
				{
#if SERVERSIDE
                    _lock.Enter(ref lockTaken);
#endif
					if ( this._index < buffers.Length )
					{
						buffer = buffers[this._index];
						buffers[this._index++] = null;
						allocateBuffer = buffer == null;
					}
				}
				finally
				{
#if SERVERSIDE
                    if(lockTaken)
                        _lock.Exit(false);
#endif
				}

				// While we were holding the lock, we grabbed whatever was at the next available index, if
				// there was one.  If we tried and if we got back null, that means we hadn't yet allocated
				// for that slot, in which case we should do so now.
				if ( allocateBuffer )
				{
					buffer = new T[this.bufferLength];
#if SERVERSIDE
                    var log = ArrayPoolEventSource.Log;
                    if(log.IsEnabled()) {
                        log.BufferAllocated(buffer.GetHashCode(), _bufferLength, _poolId, Id,
                            ArrayPoolEventSource.BufferAllocatedReason.Pooled);
                    }
#endif
				}

				return buffer;
			}

			/// <summary>
			/// Attempts to return the buffer to the bucket.  If successful, the buffer will be stored
			/// in the bucket and true will be returned; otherwise, the buffer won't be stored, and false
			/// will be returned.
			/// </summary>
			internal void Return( T[] array )
			{
				// Check to see if the buffer is the correct size for this bucket
				if ( array.Length != this.bufferLength )
				{
					throw new ArgumentException( "array不是来自内存池" );
				}

				// While holding the spin lock, if there's room available in the bucket,
				// put the buffer into the next available slot.  Otherwise, we just drop it.
				// The try/finally is necessary to properly handle thread aborts on platforms
				// which have them.
#if SERVERSIDE
				bool lockTaken = false;
#endif
				try
				{
#if SERVERSIDE
                    _lock.Enter(ref lockTaken);
#endif
					if ( this._index != 0 )
					{
						this._buffers[--this._index] = array;
					}
				}
				finally
				{
#if SERVERSIDE
                    if(lockTaken)
                        _lock.Exit(false);
#endif
				}
			}
		}
	}

	internal static class Utilities
	{
		[System.Runtime.CompilerServices.MethodImpl( System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining )]
		internal static int SelectBucketIndex( int bufferSize )
		{

			uint bitsRemaining = ( ( uint )bufferSize - 1 ) >> 4;

			int poolIndex = 0;
			if ( bitsRemaining > 0xFFFF )
			{
				bitsRemaining >>= 16;
				poolIndex = 16;
			}
			if ( bitsRemaining > 0xFF )
			{
				bitsRemaining >>= 8;
				poolIndex += 8;
			}
			if ( bitsRemaining > 0xF )
			{
				bitsRemaining >>= 4;
				poolIndex += 4;
			}
			if ( bitsRemaining > 0x3 )
			{
				bitsRemaining >>= 2;
				poolIndex += 2;
			}
			if ( bitsRemaining > 0x1 )
			{
				bitsRemaining >>= 1;
				poolIndex += 1;
			}

			return poolIndex + ( int )bitsRemaining;
		}

		[System.Runtime.CompilerServices.MethodImpl( System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining )]
		internal static int GetMaxSizeForBucket( int binIndex )
		{
			int maxSize = 16 << binIndex;
			return maxSize;
		}
	}

}
