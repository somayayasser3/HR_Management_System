from flask import Flask, request, jsonify
from flask_cors import CORS
import face_recognition
import numpy as np
from PIL import Image
import io

app = Flask(__name__)
CORS(app)

def get_face_encoding_from_bytes(image_bytes):
    """Get face encoding from image bytes"""
    try:
        # Convert bytes to PIL Image
        image = Image.open(io.BytesIO(image_bytes))
        # Convert PIL to numpy array for face_recognition
        image_array = np.array(image)
        
        # Convert to RGB if needed (face_recognition expects RGB)
        if len(image_array.shape) == 3 and image_array.shape[2] == 4:
            # If it's RGBA, convert to RGB
            image_array = image_array[:, :, :3]
        
        encodings = face_recognition.face_encodings(image_array)
        if not encodings:
            return None, "No face found in the image. Try a clearer image."
        return encodings[0], None
    except Exception as e:
        return None, f"Error processing image: {str(e)}"

@app.route('/compare-faces', methods=['POST'])
def compare_faces():
    """Compare two face images to determine if they are the same person"""
    try:
        # Get files from request
        image1_file = request.files.get('image1')
        image2_file = request.files.get('image2')
        
        # Handle case where files might have empty string keys (Postman issue)
        if not image1_file or not image2_file:
            all_files = []
            for key in request.files.keys():
                file_list = request.files.getlist(key)
                all_files.extend(file_list)
            
            if len(all_files) >= 2:
                image1_file = all_files[0]
                image2_file = all_files[1]
        
        # Validate files exist
        if not image1_file or not image2_file:
            return jsonify({
                'success': False,
                'error': 'Both image1 and image2 files are required'
            }), 400
        
        # Read image bytes
        image1_bytes = image1_file.read()
        image2_bytes = image2_file.read()
        
        # Check if files are not empty
        if len(image1_bytes) == 0 or len(image2_bytes) == 0:
            return jsonify({
                'success': False,
                'error': 'One or both image files are empty'
            }), 400
        
        # Get face encodings
        encoding1, error1 = get_face_encoding_from_bytes(image1_bytes)
        if error1:
            return jsonify({
                'success': False,
                'error': f'Image 1: {error1}'
            }), 400
            
        encoding2, error2 = get_face_encoding_from_bytes(image2_bytes)
        if error2:
            return jsonify({
                'success': False,
                'error': f'Image 2: {error2}'
            }), 400
        
        # Compare faces
        results = face_recognition.compare_faces([encoding1], encoding2)
        distance = face_recognition.face_distance([encoding1], encoding2)[0]
        
        is_same_person = results[0]
        
        return jsonify({
            'success': True,
            'isSamePerson': bool(is_same_person),
            'confidence': float(1 - distance),
            'distance': float(distance),
            'message': "✅ Match! This is the same person." if is_same_person else "❌ No match. This is a different person."
        })
        
    except Exception as e:
        return jsonify({
            'success': False,
            'error': f'Server error: {str(e)}'
        }), 500

@app.route('/health', methods=['GET'])
def health_check():
    """Health check endpoint"""
    return jsonify({'status': 'healthy'})

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000, debug=True)