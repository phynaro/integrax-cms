import { useState, useCallback } from 'react';
import { useDropzone } from 'react-dropzone';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Badge } from '@/components/ui/badge';
import { 
  Upload, 
  FileText, 
  CheckCircle, 
  XCircle, 
  AlertTriangle,
  Download
} from 'lucide-react';
import { ImportValidationResult, ImportRowPreview } from '../types';
import { useValidateImport, useConfirmImport, useDownloadTemplate } from '../hooks';

interface Portfolio {
  id: string;
  name: string;
}

interface ImportWizardProps {
  portfolios: Portfolio[];
  onComplete: () => void;
}

type Step = 'upload' | 'preview' | 'complete';

export function ImportWizard({ portfolios, onComplete }: ImportWizardProps) {
  const [step, setStep] = useState<Step>('upload');
  const [selectedPortfolio, setSelectedPortfolio] = useState<string>('');
  const [file, setFile] = useState<File | null>(null);
  const [validationResult, setValidationResult] = useState<ImportValidationResult | null>(null);
  const [editedRows, setEditedRows] = useState<ImportRowPreview[]>([]);

  const validateMutation = useValidateImport();
  const confirmMutation = useConfirmImport();
  const downloadTemplate = useDownloadTemplate();

  const onDrop = useCallback((acceptedFiles: File[]) => {
    if (acceptedFiles.length > 0) {
      setFile(acceptedFiles[0]);
    }
  }, []);

  const { getRootProps, getInputProps, isDragActive } = useDropzone({
    onDrop,
    accept: { 'text/csv': ['.csv'] },
    maxFiles: 1,
  });

  const handleValidate = async () => {
    if (!file || !selectedPortfolio) return;
    
    try {
      const result = await validateMutation.mutateAsync({ portfolioId: selectedPortfolio, file });
      setValidationResult(result);
      setEditedRows(result.rows);
      setStep('preview');
    } catch (error) {
      console.error('Validation failed:', error);
    }
  };

  const handleConfirm = async () => {
    if (!validationResult || !selectedPortfolio) return;
    
    try {
      await confirmMutation.mutateAsync({
        portfolioId: selectedPortfolio,
        filename: validationResult.filename,
        rows: editedRows.filter(r => r.isValid),
      });
      setStep('complete');
    } catch (error) {
      console.error('Import failed:', error);
    }
  };

  const validRows = editedRows.filter(r => r.isValid).length;
  const invalidRows = editedRows.filter(r => !r.isValid).length;

  return (
    <div className="space-y-6">
      {step === 'upload' && (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center justify-between">
              <span>Upload CSV File</span>
              <Button 
                variant="outline" 
                size="sm"
                onClick={() => downloadTemplate.mutate()}
                disabled={downloadTemplate.isPending}
              >
                <Download className="h-4 w-4 mr-2" />
                Download Template
              </Button>
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="space-y-2">
              <label className="text-sm font-medium">Portfolio</label>
              <Select value={selectedPortfolio} onValueChange={setSelectedPortfolio}>
                <SelectTrigger>
                  <SelectValue placeholder="Select portfolio" />
                </SelectTrigger>
                <SelectContent>
                  {portfolios.map(p => (
                    <SelectItem key={p.id} value={p.id}>{p.name}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div
              {...getRootProps()}
              className={`border-2 border-dashed rounded-none p-8 text-center cursor-pointer transition-colors ${
                isDragActive ? 'border-primary bg-primary/5' : 'border-muted-foreground/25 hover:border-primary'
              }`}
            >
              <input {...getInputProps()} />
              <Upload className="h-10 w-10 mx-auto mb-4 text-muted-foreground" />
              {file ? (
                <div className="flex items-center justify-center gap-2">
                  <FileText className="h-5 w-5" />
                  <span className="font-medium">{file.name}</span>
                </div>
              ) : (
                <>
                  <p className="text-lg font-medium">Drop your CSV file here</p>
                  <p className="text-sm text-muted-foreground">or click to browse</p>
                </>
              )}
            </div>

            <Button 
              onClick={handleValidate} 
              disabled={!file || !selectedPortfolio || validateMutation.isPending}
              className="w-full"
            >
              {validateMutation.isPending ? 'Validating...' : 'Validate File'}
            </Button>
          </CardContent>
        </Card>
      )}

      {step === 'preview' && validationResult && (
        <Card>
          <CardHeader>
            <CardTitle>Preview Import Data</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="flex gap-4">
              <div className="flex items-center gap-2">
                <CheckCircle className="h-5 w-5 text-green-500" />
                <span>{validRows} valid rows</span>
              </div>
              {invalidRows > 0 && (
                <div className="flex items-center gap-2">
                  <XCircle className="h-5 w-5 text-red-500" />
                  <span>{invalidRows} invalid rows</span>
                </div>
              )}
            </div>

            {invalidRows > 0 && (
              <div className="p-4 bg-red-50 border border-red-200 rounded-none flex items-start gap-2">
                <AlertTriangle className="h-5 w-5 text-red-600 mt-0.5" />
                <p className="text-red-800">
                  {invalidRows} rows have errors and will not be imported. Review the errors below.
                </p>
              </div>
            )}

            <div className="max-h-96 overflow-auto border rounded-none">
              <table className="w-full text-sm">
                <thead className="bg-muted sticky top-0">
                  <tr>
                    <th className="p-2 text-left">Row</th>
                    <th className="p-2 text-left">Status</th>
                    <th className="p-2 text-left">External ID</th>
                    <th className="p-2 text-left">Name</th>
                    <th className="p-2 text-left">Account #</th>
                    <th className="p-2 text-left">Amount</th>
                    <th className="p-2 text-left">Errors</th>
                  </tr>
                </thead>
                <tbody>
                  {editedRows.slice(0, 50).map((row) => (
                    <tr key={row.rowNumber} className={!row.isValid ? 'bg-red-50' : ''}>
                      <td className="p-2">{row.rowNumber}</td>
                      <td className="p-2">
                        {row.isValid ? (
                          <Badge variant="outline">Valid</Badge>
                        ) : (
                          <Badge variant="destructive">Error</Badge>
                        )}
                      </td>
                      <td className="p-2">{row.externalId}</td>
                      <td className="p-2">
                        {row.debtorType === 'Company' 
                          ? row.companyName 
                          : `${row.firstName} ${row.lastName}`.trim()}
                      </td>
                      <td className="p-2">{row.accountNumber}</td>
                      <td className="p-2">${row.currentBalance.toFixed(2)}</td>
                      <td className="p-2 text-red-600">{row.errors.join(', ')}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            <div className="flex gap-2">
              <Button variant="outline" onClick={() => setStep('upload')}>
                Back
              </Button>
              <Button 
                onClick={handleConfirm} 
                disabled={validRows === 0 || confirmMutation.isPending}
              >
                {confirmMutation.isPending ? 'Importing...' : `Import ${validRows} rows`}
              </Button>
            </div>
          </CardContent>
        </Card>
      )}

      {step === 'complete' && (
        <Card>
          <CardContent className="py-12 text-center">
            <CheckCircle className="h-16 w-16 mx-auto mb-4 text-green-500" />
            <h2 className="text-2xl font-bold mb-2">Import Complete!</h2>
            <p className="text-muted-foreground mb-6">
              Successfully imported {validRows} accounts
            </p>
            <Button onClick={onComplete}>View Import History</Button>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
