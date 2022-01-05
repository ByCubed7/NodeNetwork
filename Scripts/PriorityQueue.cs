// Yoinked from Stack Overflow

using System;
using System.Collections;
using System.Collections.Generic;

namespace ByCubed7.PriorityQueue {

    public class PriorityQueueElement {
        public float priority;
        public int index;
    }

    public sealed class PriorityQueue <T> where T : PriorityQueueElement {
        private int _numElements;
        private T[] _elements;

        public PriorityQueue(int maxElements) {
            _numElements = 0;
            _elements = new T[maxElements + 1];
        }

        public int Count {
            get {
                return _numElements;
            }
        }

        public int Max {
            get {
                return _elements.Length - 1;
            }
        }

        public void Enqueue(T element, float priority) {
            element.priority = priority;
            _numElements++;
            _elements[_numElements] = element;
            element.index = _numElements;
            CascadeUp(element);
        }

        private void CascadeUp(T element) {
            //aka Heapify-up
            int parent;
            if (element.index > 1) {
                parent = element.index >> 1;
                T parentElement = _elements[parent];
                if (HasHigherOrEqualPriority(parentElement, element))
                    return;

                // Element has lower priority value, so move parent down the heap to make room
                _elements[element.index] = parentElement;
                parentElement.index = element.index;

                element.index = parent;
            } else {
                return;
            }
            while (parent > 1) {
                parent >>= 1;
                T parentElement = _elements[parent];
                if (HasHigherOrEqualPriority(parentElement, element))
                    break;

                // Element has lower priority value, so move parent down the heap to make room
                _elements[element.index] = parentElement;
                parentElement.index = element.index;

                element.index = parent;
            }
            _elements[element.index] = element;
        }

        private void CascadeDown(T element) {
            int finalQueueIndex = element.index;
            int childLeftIndex = 2 * finalQueueIndex;

            if (childLeftIndex > _numElements) {
                return;
            }

            // Check if the left-child is higher-priority than the current element
            int childRightIndex = childLeftIndex + 1;
            T childLeft = _elements[childLeftIndex];
            if (HasHigherPriority(childLeft, element)) {
                // Check if there is a right child. Else, swap and finish.
                if (childRightIndex > _numElements) {
                    element.index = childLeftIndex;
                    childLeft.index = finalQueueIndex;
                    _elements[finalQueueIndex] = childLeft;
                    _elements[childLeftIndex] = element;
                    return;
                }
                // Check if the left-child is higher-priority than the right-child
                T childRight = _elements[childRightIndex];
                if (HasHigherPriority(childLeft, childRight)) {
                    // left is highest, move it up and continue
                    childLeft.index = finalQueueIndex;
                    _elements[finalQueueIndex] = childLeft;
                    finalQueueIndex = childLeftIndex;
                } else {
                    // right is even higher, move it up and continue
                    childRight.index = finalQueueIndex;
                    _elements[finalQueueIndex] = childRight;
                    finalQueueIndex = childRightIndex;
                }
            }
            // Not swapping with left-child, does right-child exist?
            else if (childRightIndex > _numElements) {
                return;
            } else {
                // Check if the right-child is higher-priority than the current element
                T childRight = _elements[childRightIndex];
                if (HasHigherPriority(childRight, element)) {
                    childRight.index = finalQueueIndex;
                    _elements[finalQueueIndex] = childRight;
                    finalQueueIndex = childRightIndex;
                }
                // Neither child is higher-priority than current, so finish and stop.
                else {
                    return;
                }
            }

            while (true) {
                childLeftIndex = 2 * finalQueueIndex;

                // If leaf element, we're done
                if (childLeftIndex > _numElements) {
                    element.index = finalQueueIndex;
                    _elements[finalQueueIndex] = element;
                    break;
                }

                // Check if the left-child is higher-priority than the current element
                childRightIndex = childLeftIndex + 1;
                childLeft = _elements[childLeftIndex];
                if (HasHigherPriority(childLeft, element)) {
                    // Check if there is a right child. Else, swap and finish.
                    if (childRightIndex > _numElements) {
                        element.index = childLeftIndex;
                        childLeft.index = finalQueueIndex;
                        _elements[finalQueueIndex] = childLeft;
                        _elements[childLeftIndex] = element;
                        break;
                    }
                    // Check if the left-child is higher-priority than the right-child
                    T childRight = _elements[childRightIndex];
                    if (HasHigherPriority(childLeft, childRight)) {
                        // left is highest, move it up and continue
                        childLeft.index = finalQueueIndex;
                        _elements[finalQueueIndex] = childLeft;
                        finalQueueIndex = childLeftIndex;
                    } else {
                        // right is even higher, move it up and continue
                        childRight.index = finalQueueIndex;
                        _elements[finalQueueIndex] = childRight;
                        finalQueueIndex = childRightIndex;
                    }
                }
                // Not swapping with left-child, does right-child exist?
                else if (childRightIndex > _numElements) {
                    element.index = finalQueueIndex;
                    _elements[finalQueueIndex] = element;
                    break;
                } else {
                    // Check if the right-child is higher-priority than the current element
                    T childRight = _elements[childRightIndex];
                    if (HasHigherPriority(childRight, element)) {
                        childRight.index = finalQueueIndex;
                        _elements[finalQueueIndex] = childRight;
                        finalQueueIndex = childRightIndex;
                    }
                    // Neither child is higher-priority than current, so finish and stop.
                    else {
                        element.index = finalQueueIndex;
                        _elements[finalQueueIndex] = element;
                        break;
                    }
                }
            }
        }

        private bool HasHigherPriority(T higher, T lower) => (higher.priority < lower.priority);
        private bool HasHigherOrEqualPriority(T higher, T lower) => (higher.priority <= lower.priority);

        public T Dequeue() {
            T returnMe = _elements[1];
            // If the element is already the last element, we can remove it immediately
            if (_numElements == 1) {
                _elements[1] = null;
                _numElements = 0;
                return returnMe;
            }

            // Swap the element with the last element
            T formerLastElement = _elements[_numElements];
            _elements[1] = formerLastElement;
            formerLastElement.index = 1;
            _elements[_numElements] = null;
            _numElements--;

            // Now bubble formerLastElement (which is no longer the last element) down
            CascadeDown(formerLastElement);
            return returnMe;
        }

        public void Resize(int maxElements) {
            T[] newArray = new T[maxElements + 1];
            int highestIndexToCopy = Math.Min(maxElements, _numElements);
            Array.Copy(_elements, newArray, highestIndexToCopy + 1);
            _elements = newArray;
        }

        public void UpdatePriority(T element, float priority) {
            element.priority = priority;
            OnElementUpdated(element);
        }

        private void OnElementUpdated(T element) {
            // Bubble the updated element up or down as appropriate
            int parentIndex = element.index >> 1;

            if (parentIndex > 0 && HasHigherPriority(element, _elements[parentIndex])) {
                CascadeUp(element);
            } else {
                // Note that CascadeDown will be called if parentElement == element (that is, element is the root)
                CascadeDown(element);
            }
        }

		public bool Contains(T element) => _elements[element.index] == element;

    }



}
